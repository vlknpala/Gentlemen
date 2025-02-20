// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sayfa yüklendiğinde çalışacak kodlar
document.addEventListener('DOMContentLoaded', function() {
    // Bülten aboneliği formu
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function(e) {
            e.preventDefault();
            const emailInput = this.querySelector('input[type="email"]');
            if (emailInput.value) {
                // TODO: API entegrasyonu
                alert('Bülten aboneliğiniz başarıyla gerçekleşti!');
                emailInput.value = '';
            }
        });
    }

    // Lazy loading için IntersectionObserver
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img.lazy').forEach(img => imageObserver.observe(img));
    }

    // Smooth scroll
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Navbar scroll efekti
    let lastScroll = 0;
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            const currentScroll = window.pageYOffset;
            if (currentScroll <= 0) {
                navbar.classList.remove('scroll-up');
                return;
            }

            if (currentScroll > lastScroll && !navbar.classList.contains('scroll-down')) {
                navbar.classList.remove('scroll-up');
                navbar.classList.add('scroll-down');
            } else if (currentScroll < lastScroll && navbar.classList.contains('scroll-down')) {
                navbar.classList.remove('scroll-down');
                navbar.classList.add('scroll-up');
            }
            lastScroll = currentScroll;
        });
    }

    // Like butonu işlevselliği
    document.querySelectorAll('.like-button').forEach(button => {
        button.addEventListener('click', async function(e) {
            e.preventDefault();
            const tipId = this.dataset.tipId;
            try {
                const response = await fetch(`/StyleTip/Like/${tipId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
                
                if (response.ok) {
                    const data = await response.json();
                    const likeCount = this.querySelector('.like-count');
                    if (likeCount) {
                        likeCount.textContent = data.likes;
                    }
                    this.classList.toggle('liked');
                }
            } catch (error) {
                console.error('Beğeni işlemi başarısız:', error);
            }
        });
    });

    // Kategori filtreleme
    const categoryButtons = document.querySelectorAll('.category-filter');
    categoryButtons.forEach(button => {
        button.addEventListener('click', function() {
            const category = this.dataset.category;
            const items = document.querySelectorAll('.filterable-item');
            
            categoryButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            items.forEach(item => {
                if (category === 'all' || item.dataset.category === category) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    });

    // Arama kutusu otomatik tamamlama
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        let timeoutId;
        searchInput.addEventListener('input', function() {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(async () => {
                const query = this.value.trim();
                if (query.length >= 2) {
                    try {
                        const response = await fetch(`/api/search?q=${encodeURIComponent(query)}`);
                        if (response.ok) {
                            const results = await response.json();
                            updateSearchResults(results);
                        }
                    } catch (error) {
                        console.error('Arama hatası:', error);
                    }
                }
            }, 300);
        });
    }
});

// Arama sonuçlarını güncelleme
function updateSearchResults(results) {
    const resultsContainer = document.querySelector('.search-results');
    if (resultsContainer) {
        resultsContainer.innerHTML = '';
        if (results.length > 0) {
            results.forEach(result => {
                const item = document.createElement('div');
                item.className = 'search-result-item';
                item.innerHTML = `
                    <a href="${result.url}">
                        <img src="${result.imageUrl}" alt="${result.title}">
                        <div>
                            <h4>${result.title}</h4>
                            <p>${result.description}</p>
                        </div>
                    </a>
                `;
                resultsContainer.appendChild(item);
            });
            resultsContainer.style.display = 'block';
        } else {
            resultsContainer.style.display = 'none';
        }
    }
}

// Mobil menü toggle
function toggleMobileMenu() {
    const mobileMenu = document.querySelector('.mobile-menu');
    if (mobileMenu) {
        mobileMenu.classList.toggle('active');
    }
}

// Resim önizleme
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
