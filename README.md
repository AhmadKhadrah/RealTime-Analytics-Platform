# ⚡ Real-Time Analytics & Streaming Platform

A high-throughput, real-time analytics and data streaming platform built using modern **Clean Architecture**, **.NET 8**, **Redis Pub/Sub**, **SignalR**, and **React (Vite + TypeScript)**.

This system simulates high-frequency user interactions, processes events asynchronously with low-latency caching, and streams aggregated metrics to a live interactive dashboard.

---

## 🏛️ System Architecture Overview

### Key Architectural Highlights:
* **Clean Architecture Principle:** Separation of concerns using `Domain`, `Application`, `Infrastructure`, and `Presentation` layers.
* **Pub/Sub Messaging Pattern:** Decouples event generation from downstream analytics ingestion via Redis Channels.
* **Background Processing:** Background Worker continuously consumes stream items, computes aggregate metrics, and updates Redis in-memory storage.
* **Real-Time Web Push:** SignalR pushes updated aggregate stats over persistent WebSockets to connected web interfaces with zero polling overhead.

---

## 🛠️ Tech Stack & Tools

* **Backend Framework:** .NET 8 (ASP.NET Core Web API & Worker Service)
* **Frontend Framework:** React 18, Vite, TypeScript, Tailwind CSS
* **Data Visualization:** Recharts, Lucide Icons
* **Messaging & Caching:** Redis (StackExchange.Redis)
* **Real-Time Communication:** Microsoft SignalR (WebSockets)

---

## 🚀 Getting Started Locally

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Node.js (v18+) & NPM](https://nodejs.org/)
* [Redis Server](https://redis.io/) (Running on `localhost:6379`)

### Execution Steps

1. **Clone the Repository:**
   ```bash
   git clone [https://github.com/AhmadKhadrah/RealTime-Analytics-Platform.git](https://github.com/AhmadKhadrah/RealTime-Analytics-Platform.git)
   cd RealTime-Analytics-Platform