/**
 * Multiple Image Uploader
 * 
 * Çoklu fotoğraf yükleme ve yönetimi için JavaScript bileşeni
 */
class MultipleImageUploader {
    constructor(options) {
        this.options = Object.assign({
            container: null,
            uploadUrl: '/Admin/UploadMultipleImages',
            saveUrl: '/Admin/SaveImagesToEntity',
            getImagesUrl: '/Admin/GetEntityImages',
            deleteImageUrl: '/Admin/DeleteEntityImage',
            setMainImageUrl: '/Admin/SetMainImage',
            reorderImagesUrl: '/Admin/ReorderImages',
            entityType: '',
            entityId: 0,
            maxFileSize: 5, // MB
            maxFiles: 10,
            allowedTypes: ['image/jpeg', 'image/png', 'image/gif'],
            onUploadComplete: null,
            onError: null,
            onSuccess: null,
            onDelete: null,
            onSetMain: null,
            onReorder: null
        }, options);

        this.uploadedFiles = [];
        this.existingImages = [];
        this.container = typeof this.options.container === 'string' 
            ? document.querySelector(this.options.container) 
            : this.options.container;

        if (!this.container) {
            console.error('Container element not found');
            return;
        }

        this.init();
    }

    init() {
        this.createUploader();
        
        if (this.options.entityId > 0) {
            this.loadExistingImages();
        }
    }

    createUploader() {
        // Ana container
        this.container.innerHTML = `
            <div class="multiple-image-uploader">
                <div class="upload-area" id="uploadArea">
                    <div class="upload-area-inner">
                        <i class="fas fa-cloud-upload-alt fa-3x mb-3"></i>
                        <p>Fotoğrafları buraya sürükleyin veya <span>dosya seçin</span></p>
                        <p class="text-muted small">Maksimum dosya boyutu: ${this.options.maxFileSize}MB, İzin verilen formatlar: JPG, PNG, GIF</p>
                        <input type="file" id="fileInput" multiple accept="image/*" style="display: none;">
                    </div>
                </div>
                <div class="image-preview-container mt-3" id="imagePreviewContainer">
                    <h6 class="mb-3">Yüklenen Görseller <small class="text-muted">(Ana görsel olarak ayarlamak için görsele tıklayın)</small></h6>
                    <div class="image-preview-list" id="imagePreviewList"></div>
                </div>
            </div>
        `;

        // Elementleri seç
        this.uploadArea = this.container.querySelector('#uploadArea');
        this.fileInput = this.container.querySelector('#fileInput');
        this.imagePreviewList = this.container.querySelector('#imagePreviewList');

        // Event listener'ları ekle
        this.uploadArea.addEventListener('click', () => this.fileInput.click());
        this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        
        // Drag & Drop desteği
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            this.uploadArea.addEventListener(eventName, this.preventDefaults, false);
        });
        
        ['dragenter', 'dragover'].forEach(eventName => {
            this.uploadArea.addEventListener(eventName, () => {
                this.uploadArea.classList.add('highlight');
            }, false);
        });
        
        ['dragleave', 'drop'].forEach(eventName => {
            this.uploadArea.addEventListener(eventName, () => {
                this.uploadArea.classList.remove('highlight');
            }, false);
        });
        
        this.uploadArea.addEventListener('drop', (e) => {
            const dt = e.dataTransfer;
            const files = dt.files;
            this.handleFiles(files);
        }, false);

        // Sortable.js ile sıralama
        if (typeof Sortable !== 'undefined') {
            this.sortable = new Sortable(this.imagePreviewList, {
                animation: 150,
                ghostClass: 'sortable-ghost',
                onEnd: () => {
                    this.updateImageOrder();
                }
            });
        }
    }

    preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    handleFileSelect(e) {
        const files = e.target.files;
        this.handleFiles(files);
    }

    handleFiles(files) {
        if (!files || files.length === 0) return;
        
        // Dosya sayısı kontrolü
        if (this.uploadedFiles.length + this.existingImages.length + files.length > this.options.maxFiles) {
            this.showError(`En fazla ${this.options.maxFiles} dosya yükleyebilirsiniz.`);
            return;
        }
        
        // Dosyaları kontrol et ve yükle
        const validFiles = Array.from(files).filter(file => this.validateFile(file));
        
        if (validFiles.length > 0) {
            this.uploadFiles(validFiles);
        }
    }

    validateFile(file) {
        // Dosya tipi kontrolü
        if (!this.options.allowedTypes.includes(file.type)) {
            this.showError(`Geçersiz dosya tipi: ${file.type}. İzin verilen tipler: ${this.options.allowedTypes.join(', ')}`);
            return false;
        }
        
        // Dosya boyutu kontrolü
        const fileSizeMB = file.size / (1024 * 1024);
        if (fileSizeMB > this.options.maxFileSize) {
            this.showError(`Dosya boyutu çok büyük: ${fileSizeMB.toFixed(2)}MB. Maksimum izin verilen boyut: ${this.options.maxFileSize}MB`);
            return false;
        }
        
        return true;
    }

    async uploadFiles(files) {
        const formData = new FormData();
        
        for (const file of files) {
            formData.append('files', file);
        }
        
        try {
            // Yükleme göstergesi
            this.uploadArea.classList.add('uploading');
            
            const response = await fetch(this.options.uploadUrl, {
                method: 'POST',
                body: formData
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Yüklenen dosyaları önizleme listesine ekle
                for (const filePath of result.filePaths) {
                    this.uploadedFiles.push({
                        url: filePath,
                        isMain: this.uploadedFiles.length === 0 && this.existingImages.length === 0
                    });
                }
                
                this.renderPreviewImages();
                
                // Callback
                if (typeof this.options.onUploadComplete === 'function') {
                    this.options.onUploadComplete(result.filePaths);
                }
            } else {
                this.showError(result.message || 'Dosya yükleme hatası');
            }
        } catch (error) {
            this.showError('Dosya yükleme sırasında bir hata oluştu: ' + error.message);
        } finally {
            this.uploadArea.classList.remove('uploading');
        }
    }

    renderPreviewImages() {
        // Önce mevcut görselleri göster
        let html = '';
        
        // Mevcut görseller
        this.existingImages.forEach((image, index) => {
            html += this.createImagePreviewHtml(image, index, true);
        });
        
        // Yeni yüklenen görseller
        this.uploadedFiles.forEach((file, index) => {
            html += this.createImagePreviewHtml(file, index, false);
        });
        
        this.imagePreviewList.innerHTML = html;
        
        // Event listener'ları ekle
        this.addImageEventListeners();
    }

    createImagePreviewHtml(image, index, isExisting) {
        const id = isExisting ? `existing-${image.id}` : `new-${index}`;
        const mainClass = image.isMain ? 'is-main' : '';
        
        return `
            <div class="image-preview-item ${mainClass}" data-id="${id}" data-url="${image.url}" data-is-existing="${isExisting}" ${isExisting ? `data-image-id="${image.id}"` : ''}>
                <div class="image-preview">
                    <img src="${image.url}" alt="Preview">
                    <div class="image-actions">
                        <button type="button" class="btn btn-sm btn-danger delete-image" title="Görseli Sil">
                            <i class="fas fa-trash"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-primary set-main-image" title="Ana Görsel Yap">
                            <i class="fas fa-star"></i>
                        </button>
                    </div>
                </div>
                <div class="image-info">
                    ${image.isMain ? '<span class="badge bg-success">Ana Görsel</span>' : ''}
                </div>
            </div>
        `;
    }

    addImageEventListeners() {
        // Silme butonları
        const deleteButtons = this.container.querySelectorAll('.delete-image');
        deleteButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.stopPropagation();
                const item = button.closest('.image-preview-item');
                this.deleteImage(item);
            });
        });
        
        // Ana görsel yapma butonları
        const setMainButtons = this.container.querySelectorAll('.set-main-image');
        setMainButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.stopPropagation();
                const item = button.closest('.image-preview-item');
                this.setMainImage(item);
            });
        });
        
        // Görsel tıklama (ana görsel yapma)
        const imageItems = this.container.querySelectorAll('.image-preview-item');
        imageItems.forEach(item => {
            item.addEventListener('click', () => {
                this.setMainImage(item);
            });
        });
    }

    async deleteImage(item) {
        const isExisting = item.dataset.isExisting === 'true';
        const id = item.dataset.id;
        const url = item.dataset.url;
        
        if (isExisting) {
            // Veritabanından sil
            const imageId = parseInt(item.dataset.imageId);
            
            try {
                const response = await fetch(this.options.deleteImageUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ imageId })
                });
                
                const result = await response.json();
                
                if (result.success) {
                    // Listeden kaldır
                    this.existingImages = this.existingImages.filter(img => img.id !== imageId);
                    item.remove();
                    
                    // Callback
                    if (typeof this.options.onDelete === 'function') {
                        this.options.onDelete(imageId);
                    }
                } else {
                    this.showError(result.message || 'Görsel silme hatası');
                }
            } catch (error) {
                this.showError('Görsel silme sırasında bir hata oluştu: ' + error.message);
            }
        } else {
            // Yeni yüklenen görseli listeden kaldır
            const index = parseInt(id.split('-')[1]);
            this.uploadedFiles.splice(index, 1);
            item.remove();
            
            // Yeniden render et
            this.renderPreviewImages();
        }
    }

    setMainImage(item) {
        const isExisting = item.dataset.isExisting === 'true';
        
        if (isExisting) {
            // Veritabanında ana görseli güncelle
            const imageId = parseInt(item.dataset.imageId);
            this.updateMainImage(imageId);
        } else {
            // Yeni yüklenen görseller arasında ana görseli güncelle
            const index = parseInt(item.dataset.id.split('-')[1]);
            
            // Tüm görsellerin ana görsel işaretini kaldır
            this.existingImages.forEach(img => img.isMain = false);
            this.uploadedFiles.forEach(file => file.isMain = false);
            
            // Seçilen görseli ana görsel yap
            this.uploadedFiles[index].isMain = true;
            
            // Yeniden render et
            this.renderPreviewImages();
        }
    }

    async updateMainImage(imageId) {
        try {
            const response = await fetch(this.options.setMainImageUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ imageId })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Mevcut görselleri güncelle
                this.existingImages.forEach(img => {
                    img.isMain = (img.id === imageId);
                });
                
                // Yeni yüklenen görsellerin ana görsel işaretini kaldır
                this.uploadedFiles.forEach(file => file.isMain = false);
                
                // Yeniden render et
                this.renderPreviewImages();
                
                // Callback
                if (typeof this.options.onSetMain === 'function') {
                    this.options.onSetMain(imageId);
                }
            } else {
                this.showError(result.message || 'Ana görsel güncelleme hatası');
            }
        } catch (error) {
            this.showError('Ana görsel güncelleme sırasında bir hata oluştu: ' + error.message);
        }
    }

    updateImageOrder() {
        const items = this.imagePreviewList.querySelectorAll('.image-preview-item');
        const existingImageIds = [];
        
        items.forEach(item => {
            if (item.dataset.isExisting === 'true') {
                existingImageIds.push(parseInt(item.dataset.imageId));
            }
        });
        
        if (existingImageIds.length > 0) {
            this.saveImageOrder(existingImageIds);
        }
    }

    async saveImageOrder(imageIds) {
        try {
            const response = await fetch(this.options.reorderImagesUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ imageIds })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Callback
                if (typeof this.options.onReorder === 'function') {
                    this.options.onReorder(imageIds);
                }
            } else {
                this.showError(result.message || 'Görsel sıralama hatası');
            }
        } catch (error) {
            this.showError('Görsel sıralama sırasında bir hata oluştu: ' + error.message);
        }
    }

    async loadExistingImages() {
        try {
            const response = await fetch(`${this.options.getImagesUrl}?entityType=${this.options.entityType}&entityId=${this.options.entityId}`);
            const result = await response.json();
            
            if (result.success) {
                this.existingImages = result.images;
                this.renderPreviewImages();
            } else {
                console.error('Mevcut görseller yüklenirken hata oluştu:', result.message);
            }
        } catch (error) {
            console.error('Mevcut görseller yüklenirken hata oluştu:', error);
        }
    }

    async saveImagesToEntity() {
        if (this.uploadedFiles.length === 0) {
            return { success: true, message: 'Kaydedilecek yeni görsel yok.' };
        }
        
        try {
            const imagePaths = this.uploadedFiles.map(file => file.url);
            const isMainList = this.uploadedFiles.map(file => file.isMain);
            
            const response = await fetch(this.options.saveUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    entityType: this.options.entityType,
                    entityId: this.options.entityId,
                    imagePaths,
                    isMainList
                })
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Yüklenen dosyaları temizle
                this.uploadedFiles = [];
                
                // Mevcut görselleri yeniden yükle
                await this.loadExistingImages();
                
                // Callback
                if (typeof this.options.onSuccess === 'function') {
                    this.options.onSuccess(result);
                }
            } else {
                this.showError(result.message || 'Görseller kaydedilirken bir hata oluştu');
                
                // Callback
                if (typeof this.options.onError === 'function') {
                    this.options.onError(result);
                }
            }
            
            return result;
        } catch (error) {
            const errorMessage = 'Görseller kaydedilirken bir hata oluştu: ' + error.message;
            this.showError(errorMessage);
            
            // Callback
            if (typeof this.options.onError === 'function') {
                this.options.onError({ success: false, message: errorMessage });
            }
            
            return { success: false, message: errorMessage };
        }
    }

    showError(message) {
        console.error(message);
        
        // Callback
        if (typeof this.options.onError === 'function') {
            this.options.onError({ success: false, message });
        }
    }

    // Dışarıdan erişilebilir metodlar
    getUploadedFiles() {
        return this.uploadedFiles;
    }

    getExistingImages() {
        return this.existingImages;
    }

    getAllImages() {
        return [...this.existingImages, ...this.uploadedFiles];
    }

    reset() {
        this.uploadedFiles = [];
        this.renderPreviewImages();
    }
}

// CSS stillerini ekle
const style = document.createElement('style');
style.textContent = `
    .multiple-image-uploader {
        margin-bottom: 20px;
    }
    
    .upload-area {
        border: 2px dashed #ccc;
        border-radius: 8px;
        padding: 30px;
        text-align: center;
        cursor: pointer;
        transition: all 0.3s ease;
    }
    
    .upload-area:hover, .upload-area.highlight {
        border-color: #007bff;
        background-color: rgba(0, 123, 255, 0.05);
    }
    
    .upload-area.uploading {
        opacity: 0.7;
        pointer-events: none;
    }
    
    .upload-area-inner span {
        color: #007bff;
        font-weight: 500;
    }
    
    .image-preview-list {
        display: flex;
        flex-wrap: wrap;
        gap: 15px;
        margin-top: 15px;
    }
    
    .image-preview-item {
        width: 150px;
        border-radius: 8px;
        overflow: hidden;
        box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        transition: all 0.3s ease;
        cursor: pointer;
        position: relative;
    }
    
    .image-preview-item:hover {
        transform: translateY(-5px);
        box-shadow: 0 5px 15px rgba(0,0,0,0.1);
    }
    
    .image-preview-item.is-main {
        border: 2px solid #28a745;
    }
    
    .image-preview {
        position: relative;
        height: 150px;
    }
    
    .image-preview img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }
    
    .image-actions {
        position: absolute;
        top: 5px;
        right: 5px;
        display: flex;
        gap: 5px;
        opacity: 0;
        transition: opacity 0.3s ease;
    }
    
    .image-preview-item:hover .image-actions {
        opacity: 1;
    }
    
    .image-info {
        padding: 8px;
        background-color: #f8f9fa;
        text-align: center;
    }
    
    .sortable-ghost {
        opacity: 0.5;
    }
`;
document.head.appendChild(style); 