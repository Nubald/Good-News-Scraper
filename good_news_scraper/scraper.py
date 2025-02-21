import os
import requests
from bs4 import BeautifulSoup
import json
from datetime import datetime
from urllib.parse import urljoin
import xml.etree.ElementTree as ET


class GoodNewsScraper:
    def __init__(self):
        self.base_url = "https://www.goodnewsnetwork.org"
        self.rss_urls = {
            "Good News Network": "https://www.goodnewsnetwork.org/feed/",
            "BBC Good News": "https://www.bbc.co.uk/news/10628494",
            # Add more sources here
        }
        self.categories = {
            'animals': ['animals', 'pets', 'wildlife', 'dog', 'cat', 'bird'],
            'people': ['kindness', 'heroes', 'community', 'help', 'saved'],
            'science': ['discovery', 'innovation', 'research', 'study'],
            'environment': [
                'conservation',
                'sustainability',
                'nature',
                'climate'
            ],
            'funny': ['humor', 'fun', 'joy', 'laugh', 'smile'],
            'inspiring': [
                'inspiration',
                'achievement',
                'success',
                'hope'
            ]
        }
        self.news_data = []

    def scrape_good_news_network(self):
        self.scrape_rss_feed(self.rss_urls["Good News Network"])

    def scrape_bbc_good_news(self):
        print("Fetching BBC Good News...")
        response = requests.get(self.rss_urls["BBC Good News"])
        soup = BeautifulSoup(response.text, 'html.parser')
        
        articles = soup.find_all('article')
        for article in articles:
            try:
                title_elem = article.find('h3')
                if not title_elem:
                    continue
                
                title = title_elem.get_text().strip()
                link = urljoin(self.base_url, title_elem.find('a')['href'])
                date_str = datetime.now().strftime('%Y-%m-%d')
                image_url = article.find('img')['src'] if article.find('img') else ''
                
                category = self.categorize_article(title)
                
                article_data = {
                    'title': title,
                    'link': link,
                    'image': image_url,
                    'date': date_str,
                    'category': category,
                    'source': 'BBC News'
                }
                
                self.news_data.append(article_data)
                print(f"Successfully processed article: {title[:50]}")
                
            except Exception as e:
                print(f"Error processing article: {str(e)}")
                continue

    def scrape_rss_feed(self, rss_url):
        try:
            headers = {
                'User-Agent': (
                    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) '
                    'AppleWebKit/537.36 (KHTML, like Gecko) '
                    'Chrome/91.0.4472.124 Safari/537.36'
                )
            }
            print(f"Fetching RSS feed from {rss_url}")
            response = requests.get(rss_url, headers=headers)
            print(f"Response status code: {response.status_code}")
            
            # Parse RSS feed
            root = ET.fromstring(response.content)
            channel = root.find('channel')
            items = channel.findall('item')
            
            print(f"Found {len(items)} articles in RSS feed")
            
            for item in items[:20]:  # Limit to 20 articles
                try:
                    # Get title
                    title = item.find('title').text.strip()
                    print(f"Processing article: {title[:50]}...")
                    
                    # Get link
                    link = item.find('link').text.strip()
                    
                    # Get date
                    pub_date = item.find('pubDate').text
                    try:
                        date_obj = datetime.strptime(
                            pub_date,
                            '%a, %d %b %Y %H:%M:%S %z'
                        )
                        date_str = date_obj.strftime('%Y-%m-%d')
                    except ValueError:
                        date_str = datetime.now().strftime('%Y-%m-%d')
                    
                    # Get image
                    image_url = ''
                    media = item.find(
                        '{http://search.yahoo.com/mrss/}content'
                    )
                    if media is not None:
                        image_url = media.get('url', '')
                    
                    if not image_url:
                        # Try to get image from content
                        content = item.find(
                            '{http://purl.org/rss/1.0/modules/content/}encoded'
                        )
                        if content is not None:
                            content_soup = BeautifulSoup(
                                content.text,
                                'html.parser'
                            )
                            img = content_soup.find('img')
                            if img:
                                image_url = img.get('src', '')
                    
                    # If still no image, fetch the article page
                    if not image_url:
                        article_response = requests.get(
                            link,
                            headers=headers
                        )
                        article_soup = BeautifulSoup(
                            article_response.text,
                            'html.parser'
                        )
                        meta_img = article_soup.find(
                            'meta',
                            property='og:image'
                        )
                        if meta_img:
                            image_url = meta_img.get('content', '')
                    
                    # Ensure image URL is absolute
                    if image_url and not image_url.startswith('http'):
                        image_url = urljoin(self.base_url, image_url)
                    
                    # Categorize the article
                    category = self.categorize_article(title)
                    
                    article_data = {
                        'title': title,
                        'link': link,
                        'image': image_url,
                        'date': date_str,
                        'category': category,
                        'source': 'Good News Network'
                    }
                    
                    self.news_data.append(article_data)
                    print(f"Successfully processed article: {title[:50]}")
                    
                except Exception as e:
                    print(f"Error processing article: {str(e)}")
                    continue
                    
        except Exception as e:
            print(f"Error scraping Good News Network: {str(e)}")

    def categorize_article(self, title):
        title_lower = title.lower()
        
        for category, keywords in self.categories.items():
            for keyword in keywords:
                if keyword in title_lower:
                    return category
        
        return 'inspiring'  # Default category

    def save_to_json(self):
        # Save locally
        data_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'data')
        if not os.path.exists(data_dir):
            os.makedirs(data_dir)
            
        # Save current version
        json_path = os.path.join(data_dir, 'news.json')
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(
                self.news_data,
                f,
                ensure_ascii=False,
                indent=2
            )
        print(f"Saved {len(self.news_data)} articles to {json_path}")

    def run(self):
        print("Starting news scraping...")
        self.scrape_good_news_network()
        self.scrape_bbc_good_news()
        self.save_to_json()
        print(f"Scraping completed. Found {len(self.news_data)} articles.")


if __name__ == "__main__":
    scraper = GoodNewsScraper()
    scraper.run()
