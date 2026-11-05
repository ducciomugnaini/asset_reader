# PRD - Visualizzatore di asset finanziari

## 1. Executive Summary

### Problem Statement
Gli investitori privati hanno bisogno di consultare rapidamente l'andamento di ETF e bond partendo dal relativo codice ISIN, senza dover navigare manualmente tra più pagine del sito di Borsa Italiana. L'MVP deve offrire una lettura chiara dell'andamento storico per intervalli temporali predefiniti.

### Proposed Solution
Realizzare un'applicazione web con frontend React e backend C# che accetti un codice ISIN, lo validi tramite i servizi di Borsa Italiana e visualizzi il grafico dell'asset richiesto. L'utente potrà scegliere tra gli intervalli giornaliero, mensile e trimestrale e alternare tema chiaro e tema scuro.

Il backend fungerà da proxy verso gli endpoint di Borsa Italiana, gestendo il bearer token esclusivamente lato server. Il token fornito durante la discovery è considerato compromesso e deve essere revocato o ruotato prima dell'uso in qualsiasi ambiente.

### Success Criteria
Le metriche di prodotto sono `TBD`. Per l'MVP sono definiti i seguenti criteri tecnici verificabili:

- Un ISIN valido restituisce un grafico visualizzato entro 2 secondi nel 95% delle richieste su rete non congestionata, escluso il tempo di risposta del provider superiore a 1 secondo.
- Un ISIN formalmente non valido viene rifiutato dal client senza chiamata al provider.
- Un ISIN valido che non produce dati mostra un messaggio di errore comprensibile senza rompere la pagina.
- Il cambio tra giornaliero, mensile e trimestrale aggiorna il grafico senza ricaricare l'intera pagina.
- Il layout resta utilizzabile a viewport desktop e mobile e il tema scelto rimane attivo durante la sessione corrente.

## 2. User Experience & Functionality

### User Personas

- **Investitore privato**: cerca un ETF o un bond tramite ISIN e vuole verificare rapidamente il suo andamento storico.

### User Flow

1. L'utente apre l'applicazione.
2. Inserisce un codice ISIN nel campo di ricerca.
3. Seleziona l'intervallo desiderato: giornaliero, mensile o trimestrale.
4. Invia la richiesta.
5. Il backend valida l'input e chiama l'endpoint corrispondente di Borsa Italiana.
6. Il frontend visualizza nome, identificativo se disponibile, ultimo valore disponibile e serie storica nel grafico.
7. L'utente cambia intervallo e visualizza la serie aggiornata.
8. L'utente può passare dal tema chiaro al tema scuro e viceversa.

### User Stories

- **Story 1 - Apertura dell'app**: As an investitore privato, I want to aprire una schermata pronta per una nuova ricerca, so that posso iniziare senza configurazioni preliminari.
- **Story 2 - Inserimento ISIN**: As an investitore privato, I want to inserire un codice ISIN, so that posso cercare l'asset che mi interessa.
- **Story 3 - Normalizzazione input**: As an investitore privato, I want to poter inserire l'ISIN con spazi accidentali o lettere minuscole, so that non devo correggere manualmente il formato prima della ricerca.
- **Story 4 - Validazione preventiva**: As an investitore privato, I want to sapere subito se il codice inserito non rispetta il formato ISIN, so that posso correggerlo senza attendere una risposta del provider.
- **Story 5 - Visualizzazione risultato**: As an investitore privato, I want to visualizzare il nome o l'identificativo dell'asset e il suo grafico, so that posso verificare di aver cercato lo strumento corretto.
- **Story 6 - Intervallo giornaliero**: As an investitore privato, I want to vedere l'andamento giornaliero, so that posso analizzare i movimenti della giornata.
- **Story 7 - Intervallo mensile**: As an investitore privato, I want to vedere l'andamento dell'ultimo mese, so that posso osservare una prospettiva di breve periodo.
- **Story 8 - Intervallo trimestrale**: As an investitore privato, I want to vedere l'andamento degli ultimi tre mesi, so that posso osservare una prospettiva di medio periodo.
- **Story 9 - Cambio intervallo**: As an investitore privato, I want to cambiare intervallo senza reinserire l'ISIN, so that posso confrontare rapidamente le tre prospettive temporali.
- **Story 10 - Ultimo dato**: As an investitore privato, I want to visualizzare il valore dell'ultimo dato disponibile e la relativa data, so that posso interpretare correttamente il grafico.
- **Story 11 - Errori e dati assenti**: As an investitore privato, I want to ricevere un messaggio chiaro quando l'ISIN non è valido, l'asset non è disponibile o il servizio non risponde, so that posso correggere la ricerca o riprovare.
- **Story 12 - Tema e accessibilità**: As an investitore privato, I want to cambiare tema chiaro o scuro e usare i controlli da tastiera, so that posso usare l'app secondo le mie preferenze visive e operative.

### Edge Cases

- L'utente invia il form senza inserire alcun codice.
- L'utente inserisce meno o più di 12 caratteri oppure caratteri speciali.
- L'utente inserisce un ISIN formalmente valido ma non supportato da Borsa Italiana.
- Il provider restituisce una risposta valida ma senza punti dati o senza ultimo prezzo.
- Il provider restituisce un errore, va in timeout o risponde con un formato non riconoscibile.
- L'utente cambia intervallo mentre la richiesta precedente è ancora in corso.
- L'utente avvia più volte la stessa ricerca rapidamente.
- L'utente cambia tema durante il caricamento del grafico.

### Acceptance Criteria

- **AC1 - Story 1, apertura dell'app**
  - Al primo caricamento viene mostrato un campo ISIN vuoto, un intervallo predefinito e un'azione di ricerca.
  - Prima della prima ricerca non viene chiamato il provider.

- **AC2 - Story 2 e Story 3, inserimento e normalizzazione ISIN**
  - Il campo accetta un ISIN composto da 12 caratteri alfanumerici.
  - L'input viene normalizzato rimuovendo spazi iniziali e finali e convertendo il valore in maiuscolo.
  - La ricerca usa il valore normalizzato e non quello digitato originalmente.

- **AC3 - Story 4, validazione preventiva**
  - Un input vuoto o non conforme al formato ISIN viene rifiutato con un messaggio inline.
  - Per un input formalmente non valido non viene effettuata alcuna richiesta a Borsa Italiana.
  - Il messaggio identifica l'errore senza esporre dettagli tecnici o token.

- **AC4 - Story 5, visualizzazione risultato**
  - Per una risposta valida il grafico viene mostrato nell'area risultato.
  - Il risultato visualizza l'ISIN cercato e i metadati disponibili del provider.
  - I punti del grafico sono ordinati temporalmente e non vengono duplicati dal cambio intervallo.

- **AC5 - Story 6, intervallo giornaliero**
  - La richiesta usa `GET /api/instruments/{ISIN},XMIL,ISIN/intraday?resolution=1MN` tramite il backend.
  - La risposta viene trasformata in un modello dati interno indipendente dal formato del provider.
  - Il grafico mostra punti ordinati temporalmente e un'indicazione dell'ultimo valore disponibile.

- **AC6 - Story 7 e Story 8, intervalli mensile e trimestrale**
  - La richiesta mensile usa `GET /api/instruments/{ISIN},XMIL,ISIN/history/period?period=1M&adjustment=true&add-last-price=true`.
  - La richiesta trimestrale usa `GET /api/instruments/{ISIN},XMIL,ISIN/history/period?period=3M&adjustment=true&add-last-price=true`.
  - La selezione dell'intervallo attivo è sempre visibile.
  - Ogni intervallo visualizza solo i dati della relativa risposta.

- **AC7 - Story 9, cambio intervallo**
  - Il cambio intervallo mantiene l'ISIN corrente e non richiede un nuovo inserimento.
  - Il cambio intervallo mostra uno stato di caricamento e sostituisce i dati precedenti solo dopo una risposta valida.
  - Se una richiesta precedente termina dopo una richiesta più recente, la risposta precedente non sovrascrive il risultato più recente.

- **AC8 - Story 10, ultimo dato**
  - Il valore dell'ultimo punto disponibile e la relativa data sono mostrati separatamente dal grafico.
  - Se il provider non restituisce l'ultimo valore, l'interfaccia mostra un'indicazione di dato non disponibile senza inventare un valore.

- **AC9 - Story 11, errori e dati assenti**
  - Errori HTTP del provider, timeout e risposte non interpretabili vengono convertiti in un errore applicativo non tecnico.
  - Una risposta senza serie dati mostra uno stato vuoto dedicato.
  - Il pulsante di ricerca non permette richieste duplicate mentre una richiesta è in corso.
  - L'utente può riprovare senza ricaricare la pagina.

- **AC10 - Story 12, tema e accessibilità**
  - Sono disponibili tema chiaro e tema scuro.
  - Il cambio tema aggiorna sfondo, testo, controlli e colori del grafico con contrasto sufficiente.
  - La preferenza resta attiva fino alla chiusura o al refresh della sessione; la persistenza permanente è fuori scope.
  - Tutti i controlli principali sono raggiungibili e azionabili da tastiera.
  - Gli stati di caricamento e errore sono annunciabili alle tecnologie assistive.

### Non-Goals

- Nessuna registrazione, autenticazione o area personale.
- Nessuna persistenza di ISIN, preferenze, cronologia o risultati.
- Nessun portafoglio, lista preferiti, alert o confronto tra asset.
- Nessuna esecuzione di ordini o consulenza finanziaria.
- Nessun indicatore tecnico, previsione, raccomandazione o analisi generata da AI.
- Nessuna fonte dati alternativa a Borsa Italiana nell'MVP.
- Nessuna scelta libera dell'intervallo o della granularità oltre alle tre opzioni predefinite.

## 3. AI System Requirements (If Applicable)

### Applicabilità
L'MVP non include funzionalità di intelligenza artificiale. Questa sezione è quindi non applicabile.

### Tool Requirements

- Non sono richiesti modelli AI, strumenti di retrieval o API AI.
- Sono richiesti un client HTTP server-side e un componente di rendering grafici nel frontend.

### Evaluation Strategy

- Test automatici del parsing e della normalizzazione delle risposte di Borsa Italiana.
- Test di integrazione con risposte registrate per esito valido, dati assenti, errore HTTP e timeout.
- Test end-to-end del flusso inserimento ISIN -> selezione intervallo -> visualizzazione grafico.

## 4. Technical Specifications

### Architecture Overview

- **Frontend React**: presenta il form ISIN, i controlli dell'intervallo, gli stati di caricamento/errore e il grafico.
- **Backend C#**: espone un'API interna, valida l'ISIN, seleziona l'endpoint in base all'intervallo, aggiunge il bearer token e normalizza la risposta.
- **Borsa Italiana**: fornisce i dati tramite `grafici.borsaitaliana.it`.
- **Persistenza**: assente. I dati transitano in memoria durante la richiesta e non vengono salvati.

Flusso dati:

`Browser -> API C# -> Borsa Italiana -> API C# -> Browser -> Grafico React`

Il browser non deve chiamare direttamente Borsa Italiana e non deve ricevere il bearer token.

### API Backend Proposed

- `GET /api/assets/{isin}?period=daily`
- `GET /api/assets/{isin}?period=monthly`
- `GET /api/assets/{isin}?period=quarterly`

Il backend deve:

- validare il formato dell'ISIN;
- impedire che l'ISIN venga usato per costruire URL arbitrari;
- mappare `daily` a `intraday?resolution=1MN`;
- mappare `monthly` a `history/period?period=1M&adjustment=true&add-last-price=true`;
- mappare `quarterly` a `history/period?period=3M&adjustment=true&add-last-price=true`;
- applicare timeout e cancellazione delle richieste;
- restituire un contratto JSON stabile al frontend;
- non restituire mai token, header sensibili o dettagli interni del provider.

### Integration Points

- **Provider dati**: `https://grafici.borsaitaliana.it`.
- **Autorizzazione provider**: bearer token configurato tramite variabile d'ambiente o secret manager, mai hardcoded e mai incluso nel bundle React.
- **Frontend/backend**: API HTTP interna con CORS limitato all'origine del frontend negli ambienti non locali.
- **Database**: nessuno nell'MVP.
- **Autenticazione utente**: nessuna nell'MVP.
- **Configurazione ambiente**: URL provider, token, timeout e origini consentite devono essere configurabili senza modificare il codice.

### Security & Privacy

- Il bearer token condiviso durante la discovery deve essere ruotato/revocato prima del deployment.
- Il token deve essere conservato in secret manager o variabili d'ambiente protette e non committato nel repository.
- Il frontend deve comunicare solo con il backend applicativo.
- Devono essere applicati HTTPS, validazione input, allowlist degli intervalli e timeout lato server.
- I log non devono contenere il bearer token né dati sensibili degli header.
- Il backend deve applicare rate limiting almeno sulle richieste verso il provider per evitare abuso e costi operativi.
- Non vengono raccolti dati personali o memorizzate ricerche nell'MVP.
- Prima del rilascio devono essere verificati i termini d'uso e i diritti di redistribuzione dei dati e del logo di Borsa Italiana.

## 5. Risks & Roadmap

### Phased Rollout

#### MVP

- Ricerca tramite ISIN.
- Supporto a ETF e bond disponibili tramite gli endpoint indicati.
- Intervalli giornaliero, mensile e trimestrale.
- Grafico con ultimo valore e data.
- Tema chiaro/scuro.
- Gestione degli stati di caricamento, errore e dati assenti.
- Nessuna persistenza e nessuna autenticazione utente.

#### v1.1

- Cache server-side a breve durata per ridurre richieste duplicate.
- Visualizzazione di valuta, mercato, nome strumento e metadati disponibili.
- Miglioramento accessibilità, navigazione da tastiera e supporto screen reader.
- Monitoraggio di latenza, error rate e disponibilità del provider.

#### v2.0

- Confronto tra più asset.
- Watchlist locale o account utente con persistenza esplicita.
- Intervalli personalizzati e ulteriori granularità.
- Indicatori tecnici, esportazione dati e notifiche, previa verifica dei requisiti legali e del provider.

### Technical Risks

- **Contratto API non documentato o variabile**: mitigare isolando l'integrazione in un adapter C#, usando test con fixture e monitorando gli errori di parsing.
- **Token scaduto o revocato**: mitigare con configurazione esterna, gestione centralizzata dei secret e procedura di rotazione.
- **Restrizioni CORS o accesso provider**: mitigare facendo transitare le chiamate dal backend e verificando i termini d'uso prima del deployment.
- **Limiti di frequenza e indisponibilità**: mitigare con timeout, rate limiting, retry limitati solo per errori transitori e messaggi applicativi chiari.
- **Differenze nei formati per ETF e bond**: mitigare con un modello dati normalizzato e fixture rappresentative per entrambe le categorie.
- **Dati finanziari ritardati o incompleti**: mostrare sempre timestamp e stato del dato quando disponibili; non presentare il grafico come quotazione real-time senza conferma del provider.
- **Interpretazione errata dell'intervallo giornaliero**: confermare con il provider che `intraday` con `resolution=1MN` soddisfi il requisito di intervallo giornaliero e documentare il fuso orario usato.
- **Uso non autorizzato dei dati**: verificare licenze, condizioni API e permessi di redistribuzione prima del rilascio pubblico.

### Open Decisions

- Libreria React per i grafici: `TBD`.
- Versione target di .NET e modalità di hosting: `TBD`.
- Hosting frontend e backend: `TBD`.
- Metriche di prodotto e soglie di adozione: `TBD`.
- Semantica esatta del timestamp e del fuso orario restituiti dal provider: `TBD`.
