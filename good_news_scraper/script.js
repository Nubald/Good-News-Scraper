class NewsArchive {
    constructor() {
        this.newsData = [];
        this.currentCategory = 'all';
        this.container = document.getElementById('news-container');
        this.template = document.getElementById('article-template');
        this.categoryButtons = document.querySelectorAll('.category-btn');
        this.loadingElement = document.createElement('div');
        this.loadingElement.className = 'loading';
        this.loadingElement.innerHTML = `
            <div class="loading-spinner"></div>
            <p>Loading news articles...</p>
        `;

        this.init();
    }

    async init() {
        try {
            // Show loading state
            this.container.innerHTML = '';
            this.container.appendChild(this.loadingElement.cloneNode(true));
            
            // Load news data
            await this.loadNewsData();
            
            // Set up event listeners
            this.setupEventListeners();
            
            // Display initial news
            await this.displayNews();
        } catch (error) {
            console.error('Error initializing news archive:', error);
            this.showError('Unable to load news articles. Please check your connection and try again.');
        }
    }

    async loadNewsData() {
        try {
            const response = await fetch('./data/news.json');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const text = await response.text();
            try {
                const data = JSON.parse(text);
                if (!Array.isArray(data)) {
                    throw new Error('Data is not an array');
                }
                
                if (data.length === 0) {
                    throw new Error('No news articles found');
                }

                // Store the data
                this.newsData = data;
                console.log(`Successfully loaded ${this.newsData.length} articles`);
            } catch (parseError) {
                console.error('JSON parse error:', parseError);
                throw new Error('Invalid news data format');
            }
        } catch (error) {
            console.error('Error loading news data:', error);
            throw error;
        }
    }

    setupEventListeners() {
        this.categoryButtons.forEach(button => {
            button.addEventListener('click', async () => {
                try {
                    // Update active button
                    this.categoryButtons.forEach(btn => btn.classList.remove('active'));
                    button.classList.add('active');

                    // Update category and display news with animation
                    this.currentCategory = button.dataset.category;
                    this.container.style.opacity = '0';
                    await new Promise(resolve => setTimeout(resolve, 300));
                    await this.displayNews();
                    this.container.style.opacity = '1';
                } catch (error) {
                    console.error('Error handling category change:', error);
                    this.showError('Error updating category display.');
                }
            });
        });
    }

    async displayNews() {
        try {
            // Clear current content
            this.container.innerHTML = '';

            // Filter news by category
            const newsToDisplay = this.currentCategory === 'all'
                ? this.newsData
                : this.newsData.filter(article => article.category === this.currentCategory);

            if (newsToDisplay.length === 0) {
                this.showMessage('No news articles found in this category.');
                return;
            }

            // Sort news by date (newest first)
            newsToDisplay.sort((a, b) => new Date(b.date) - new Date(a.date));

            // Create document fragment for better performance
            const fragment = document.createDocumentFragment();

            // Create and append article elements
            for (let i = 0; i < newsToDisplay.length; i++) {
                const article = newsToDisplay[i];
                const articleElement = await this.createArticleElement(article);
                articleElement.style.animationDelay = `${i * 0.1}s`;
                fragment.appendChild(articleElement);
            }

            // Append all articles at once
            this.container.appendChild(fragment);

        } catch (error) {
            console.error('Error displaying news:', error);
            this.showError('Error displaying news articles.');
            throw error;
        }
    }

    async createArticleElement(article) {
        try {
            const clone = this.template.content.cloneNode(true);
            const articleElement = clone.querySelector('.news-article');
            
            // Set article content
            const img = clone.querySelector('img');
            img.src = article.image || 'https://via.placeholder.com/300x200?text=No+Image';
            img.alt = article.title;
            img.onerror = () => {
                img.src = 'https://via.placeholder.com/300x200?text=No+Image';
            };

            clone.querySelector('.article-category').textContent = article.category;
            clone.querySelector('.article-title').textContent = article.title;
            
            const date = new Date(article.date).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
            clone.querySelector('.article-date').textContent = date;
            clone.querySelector('.article-source').textContent = article.source;
            
            const readMoreLink = clone.querySelector('.read-more');
            readMoreLink.href = article.link;
            readMoreLink.target = '_blank';
            readMoreLink.rel = 'noopener noreferrer';
            
            // Add animation class
            articleElement.classList.add('fade-in');

            return articleElement;
        } catch (error) {
            console.error('Error creating article element:', error, article);
            throw error;
        }
    }

    showError(message) {
        this.container.innerHTML = `
            <div class="error-message">
                <p>⚠️ ${message}</p>
                <button onclick="window.location.reload()">Try Again</button>
            </div>
        `;
    }

    showMessage(message) {
        this.container.innerHTML = `
            <div class="info-message">
                <p>${message}</p>
            </div>
        `;
    }
}

// Initialize the news archive when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new NewsArchive();
});
