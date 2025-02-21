# Good News Scraper and Archive

A web scraper that collects positive news stories and displays them in a categorized archive website. Articles are stored both locally and in Google Drive.

## Features

- Scrapes good news articles from goodnewsnetwork.org
- Automatically categorizes articles into different sections:
  - Animals
  - People
  - Science
  - Environment
  - Funny
  - Inspiring
- Responsive web interface with:
  - Category filtering
  - Clean, modern design
  - Article cards with images
  - Smooth animations
  - Mobile-friendly layout
- Google Drive Integration:
  - Automatic backup of articles
  - Timestamped JSON files
  - Organized storage in designated folder

## Setup

1. Install Python requirements:
```bash
pip install -r requirements.txt
```

2. Set up Google Drive API access:
- Follow the instructions in [README_GOOGLE_DRIVE.md](README_GOOGLE_DRIVE.md)
- Place your `credentials.json` file in the project directory

3. Run the scraper to collect news:
```bash
python scraper.py
```

4. Open the website:
- Simply open `index.html` in your web browser
- Or use a local server:
```bash
# Using Python
python -m http.server
# Then open http://localhost:8000 in your browser
```

## Project Structure

```
good_news_scraper/
├── scraper.py        # News scraper script
├── index.html        # Main website
├── styles.css        # Website styling
├── script.js         # Website functionality
├── requirements.txt  # Python dependencies
# ├── credentials.json  # Google Drive API credentials
└── data/            
    └── news.json     # Scraped news data
```

## How It Works

1. The scraper (`scraper.py`):
   - Fetches articles from goodnewsnetwork.org's RSS feed
   - Extracts title, link, image, and date
   - Categorizes articles based on content
   - Saves data locally to `data/news.json`
   - Creates backup in Google Drive with timestamp

2. The website:
   - Loads the scraped news data
   - Displays articles in a responsive grid
   - Allows filtering by category
   - Shows article details with images
   - Provides links to original articles

## Updating News

To update the news archive with fresh content:
```bash
python scraper.py
```
The website will automatically display the updated content on next load, and a backup will be saved to Google Drive.

## Storage

Articles are stored in two locations:
1. Local: `data/news.json` - Used by the website
2. Google Drive: Timestamped copies (e.g., `news_20240221_123456.json`)

## Categories

Articles are automatically categorized based on their content:
- Animals: Stories about pets, wildlife, and animal welfare
- People: Human interest stories, acts of kindness
- Science: Discoveries, innovations, research
- Environment: Conservation, sustainability, nature
- Funny: Humorous and lighthearted stories
- Inspiring: Achievement, success, hope (default category)
