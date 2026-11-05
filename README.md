# Asset Reader

Applicazione MVP per visualizzare ETF e bond tramite codice ISIN.

## Avvio locale

### Backend

Richiede .NET 10 SDK:

```powershell
cd backend\AssetReader.Api
dotnet run
```

Per aprire il backend in JetBrains Rider, apri
[AssetReader.slnx](D:/vibe_coding/asset_reader/backend/AssetReader.slnx).
Il profilo di sviluppo configura automaticamente l'API su
`http://localhost:5072`.

Il backend è organizzato in layer:

- `AssetReader.Domain`: ISIN, intervalli e modelli di dominio immutabili.
- `AssetReader.Application`: servizio applicativo e porta `IAssetDataProvider`.
- `AssetReader.Infrastructure`: adapter HTTP e mapper delle risposte Borsa Italiana.
- `AssetReader.Api`: endpoint HTTP, CORS, rate limiting e gestione errori.
- `AssetReader.Tests`: test di dominio e del mapping dei payload reali.

Configurazione opzionale:

- `BORSA_ITALIANA_BASE_URL` (default `https://grafici.borsaitaliana.it`)
- `BORSA_ITALIANA_TOKEN` (obbligatoria per le chiamate reali)
- `PROVIDER_TIMEOUT_SECONDS` (default `10`)
- `FRONTEND_ORIGIN` (default `http://localhost:5173`)

### Frontend

Richiede Node.js 18+:

```powershell
cd frontend
npm install
npm run dev
```

Il frontend usa `VITE_API_BASE_URL` per configurare il backend; in locale il
default è `http://localhost:5072`.

> Il token del provider non deve essere inserito nel frontend, nei log o nel
> repository. Deve essere ruotato/revocato prima del deployment.
