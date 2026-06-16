# URL Shortener API

A fast, secure, and scalable URL shortening service built with **.NET 10**.  
This API converts long URLs into short codes, handles redirection, tracks usage, and is optimized for production hosting on MonsterASP with Cloudflare protection.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Status](https://img.shields.io/badge/status-live-success)
![Tech](https://img.shields.io/badge/.NET-10.0-purple)
![CI - Dev](https://github.com/Nikhil767/url-shortener-api/actions/workflows/ci-dev.yml/badge.svg)
![CD - Main](https://github.com/Nikhil767/url-shortener-api/actions/workflows/cd-main-webdeploy.yml/badge.svg)

---

## 🚀 Overview

This project provides a lightweight and efficient URL shortening API that:

- Generates short, unique codes for long URLs  
- Redirects users to the original URL  
- Tracks click counts  
- Implements rate limiting  
- Supports SQL Server (local + MonsterASP hosting)  
- Uses User Secrets (local) + Environment Variables (production)  

Perfect for learning, portfolio projects, or integrating into real-world applications.

---

## 📁 Folder Structure

```
url-shortener-api/
│
├── src/
│   └── UrlShortener/
│       ├── Controllers/
│       ├── Services/
│       ├── Models/
│       ├── Middleware/
│       ├── Data/
│       ├── Program.cs
│       └── UrlShortener.csproj
│
└── tests/
    └── UrlShortener.Tests/
        ├── UnitTests/
        └── UrlShortener.Tests.csproj
```

---

## 🛠️ Tech Stack

- **.NET 10 Web API**
- **Entity Framework Core**
- **SQL Server**
- **Rate Limiting Middleware**
- **API Key Authentication**
- **User Secrets (local)**
- **Environment Variables (production)**

---

## ⚙️ Local Development Setup

### 1. Clone the repository
```bash
git clone https://github.com/Nikhil767/url-shortener-api.git
cd url-shortener-api/src/UrlShortener
```

### 2. Configure User Secrets
```bash
dotnet user-secrets init
dotnet user-secrets set "ApiSecretKey" "your-local-api-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-local-connection-string"
dotnet user-secrets set "TokenLimit" "20"
dotnet user-secrets set "TokensPerPeriod" "20"
dotnet user-secrets set "RetryAfter": "60"
dotnet user-secrets set "MaxRequestBodySize": "3"
```

### 3. Run the API
```bash
dotnet run
```

Local API URL:
```
https://localhost:5001
```

---

## 🌐 Production Setup (MonsterASP Hosting)

### Add Environment Variables

In MonsterASP Control Panel:

| Key | Value |
|-----|--------|
| ApiSecretKey | your-production-api-key |
| ConnectionStrings__DefaultConnection | your SQL connection string |
| TokenLimit | 20 |
| TokensPerPeriod | 20 |
| RetryAfter | 60 |
| MaxRequestBodySize | 3 MB |

After saving → **Recycle Application Pool**.

---

## 📡 API Endpoints

### 🔹 Shorten URL  
**POST** `/api/shorten`

Request:
```json
{
  "longUrl": "https://example.com"
}
```

Response:
```json
{
  "shortCode": "abc123",
  "shortUrl": "https://yourdomain.com/abc123"
}
```

---

### 🔹 Redirect to Original URL  
**GET** `/{shortCode}`  
Redirects to the long URL.

---

### 🔹 Get URL Details  
**GET** `/api/shorten/{shortCode}`

Response:
```json
{
  "longUrl": "https://example.com",
  "createdAt": "2026-06-15T10:00:00Z",
  "clicks": 42
}
```

---

## 🔐 Authentication

Protected endpoints require:

```
x-api-key: your-api-key
```

---

## 🧪 Running Tests

```bash
cd tests/UrlShortener.Tests
dotnet test
```

---

## Deployment Status

The API is actively hosted and monitored. You can verify the live status of the deployment below:

* **Health Check Endpoint:** [![API Status](https://img.shields.io/badge/API_Status-Live-brightgreen)](https://nikhilapi.runasp.net/alive)
##
---

## 📜 License

This project is licensed under the **MIT License**.

---

## 👨‍💻