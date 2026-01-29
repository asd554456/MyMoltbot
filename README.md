# MyMoltbot

個人工具專案

## 技術棧

- **前端**: Vue 3 (Composition API) + TypeScript + Vuetify 3
- **後端**: ASP.NET Core (.NET 10) + Entity Framework Core
- **資料庫**: PostgreSQL (Docker)
- **認證**: JWT

## 快速開始

### 1. 啟動資料庫

```bash
docker compose up -d
```

### 2. 啟動後端

```bash
cd backend
dotnet run
```

後端預設運行在 `http://localhost:5000`

### 3. 啟動前端

```bash
cd frontend
npm install
npm run dev
```

前端預設運行在 `http://localhost:5173`

## API 端點

### 認證

- `POST /api/auth/register` - 註冊
- `POST /api/auth/login` - 登入

## 專案結構

```
MyMoltbot/
├── frontend/           # Vue 3 前端
│   ├── src/
│   │   ├── components/
│   │   ├── plugins/
│   │   └── App.vue
│   └── package.json
├── backend/            # ASP.NET Core 後端
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   ├── DTOs/
│   └── Program.cs
├── docker-compose.yml  # PostgreSQL
└── README.md
```

## 環境變數

後端配置在 `backend/appsettings.json`：
- 資料庫連線字串
- JWT 設定（正式環境請更換密鑰）
