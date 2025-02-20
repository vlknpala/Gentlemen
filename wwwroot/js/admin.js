// Admin Panel JavaScript

// Sidebar Toggle
document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggle = document.getElementById('sidebarCollapse');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.getElementById('sidebar').classList.toggle('active');
        });
    }
});

// Delete Confirmation
function confirmDelete(id, type) {
    if (confirm('Bu öğeyi silmek istediğinizden emin misiniz?')) {
        $.ajax({
            url: `/Admin/Delete${type}/${id}`,
            type: 'POST',
            success: function(result) {
                if (result.success) {
                    // Remove the deleted item from the DOM
                    $(`#item-${id}`).fadeOut(function() {
                        $(this).remove();
                    });
                    // Show success message
                    showAlert('success', 'Öğe başarıyla silindi.');
                } else {
                    showAlert('danger', result.message || 'Bir hata oluştu.');
                }
            },
            error: function() {
                showAlert('danger', 'Bir hata oluştu.');
            }
        });
    }
}

// Show Alert
function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    const alertContainer = document.getElementById('alertContainer');
    if (alertContainer) {
        alertContainer.innerHTML = alertHtml;
        // Auto dismiss after 3 seconds
        setTimeout(function() {
            const alert = alertContainer.querySelector('.alert');
            if (alert) {
                $(alert).fadeOut();
            }
        }, 3000);
    }
}

// Image Preview
function previewImage(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const preview = document.querySelector('.image-preview');
            if (preview) {
                preview.src = e.target.result;
                preview.style.display = 'block';
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Data Tables
$(document).ready(function() {
    if ($.fn.DataTable) {
        $('.datatable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.24/i18n/Turkish.json'
            },
            responsive: true,
            order: [[0, 'desc']]
        });
    }
});

// Form Validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.classList.add('was-validated');
    }
    return false;
}

// Rich Text Editor
if (typeof tinymce !== 'undefined') {
    tinymce.init({
        selector: '.rich-text-editor',
        height: 300,
        plugins: [
            'advlist autolink lists link image charmap print preview anchor',
            'searchreplace visualblocks code fullscreen',
            'insertdatetime media table paste code help wordcount'
        ],
        toolbar: 'undo redo | formatselect | bold italic backcolor | \
                alignleft aligncenter alignright alignjustify | \
                bullist numlist outdent indent | removeformat | help'
    });
}

// Search Functionality
const searchInput = document.querySelector('.search-input');
if (searchInput) {
    searchInput.addEventListener('input', function() {
        const searchTerm = this.value.toLowerCase();
        const items = document.querySelectorAll('.searchable-item');
        
        items.forEach(item => {
            const text = item.textContent.toLowerCase();
            if (text.includes(searchTerm)) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    });
}

// Sort Functionality
function sortTable(columnIndex) {
    const table = document.querySelector('.sortable');
    if (!table) return;

    const rows = Array.from(table.querySelectorAll('tr:not(:first-child)'));
    const isNumeric = !isNaN(rows[0].children[columnIndex].textContent);
    
    rows.sort((a, b) => {
        const aValue = a.children[columnIndex].textContent;
        const bValue = b.children[columnIndex].textContent;
        
        if (isNumeric) {
            return parseFloat(aValue) - parseFloat(bValue);
        }
        return aValue.localeCompare(bValue);
    });
    
    rows.forEach(row => table.appendChild(row));
}

// Export to Excel
function exportToExcel(tableId, fileName) {
    const table = document.getElementById(tableId);
    if (!table) return;

    const wb = XLSX.utils.table_to_book(table);
    XLSX.writeFile(wb, `${fileName}.xlsx`);
} 