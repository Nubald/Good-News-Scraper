# Setting up Google Drive API Access

To enable Google Drive integration, follow these steps:

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google Drive API:
   - In the sidebar, click "APIs & Services" > "Library"
   - Search for "Google Drive API"
   - Click "Enable"

4. Create credentials:
   - In the sidebar, click "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth client ID"
   - Choose "Desktop application" as the application type
   - Give it a name (e.g., "Good News Scraper")
   - Click "Create"

5. Download the credentials:
   - In the OAuth 2.0 Client IDs section, find your newly created client
   - Click the download icon (⬇️)
   - Save the downloaded file as `credentials.json` in the `good_news_scraper` directory

6. First Run:
   - When you run the scraper for the first time, it will open a browser window
   - Log in with your Google account and grant the necessary permissions
   - The scraper will save the authentication token for future use

Note: The credentials.json file contains sensitive information. Do not share it or commit it to version control.

Example credentials.json structure (replace with your actual values):
```json
{
    "installed": {
        "client_id": "your-client-id.apps.googleusercontent.com",
        "project_id": "your-project-id",
        "auth_uri": "https://accounts.google.com/o/oauth2/auth",
        "token_uri": "https://oauth2.googleapis.com/token",
        "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
        "client_secret": "your-client-secret",
        "redirect_uris": ["http://localhost"]
    }
}
```

After setting up the credentials, the scraper will:
1. Save articles locally to news.json
2. Create a timestamped copy in your Google Drive folder
3. Update existing files if they have the same name
