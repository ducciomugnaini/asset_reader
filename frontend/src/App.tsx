import { FormEvent, useEffect, useMemo, useRef, useState } from "react";
import type { LineData, UTCTimestamp } from "lightweight-charts";
import { Chart, LineSeries, TimeScale, TimeScaleFitContentTrigger } from "lightweight-charts-react-components";

type Period = "daily" | "monthly" | "quarterly";
type Theme = "light" | "dark";
type AssetPoint = { time: string; value: number };
type AssetResponse = { isin: string; period: Period; name?: string; currency?: string; points: AssetPoint[] };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5072";
const periods: { key: Period; label: string }[] = [
  { key: "daily", label: "Giornaliero" },
  { key: "monthly", label: "Mensile" },
  { key: "quarterly", label: "Trimestrale" },
];

function normalizeIsin(value: string) {
  return value.trim().toUpperCase();
}

function isValidIsin(value: string) {
  return /^[A-Z]{2}[A-Z0-9]{9}[0-9]$/.test(value);
}

function formatValue(value: number, currency?: string) {
  return new Intl.NumberFormat("it-IT", { maximumFractionDigits: 4, minimumFractionDigits: 2, style: currency ? "currency" : "decimal", currency: currency ?? "EUR" }).format(value);
}

function AssetChart({ points, theme }: { points: AssetPoint[]; theme: Theme }) {
  const data = useMemo<LineData<UTCTimestamp>[]>(() => points.map((point) => ({
    time: Math.floor(new Date(point.time).getTime() / 1000) as UTCTimestamp,
    value: point.value,
  })), [points]);
  return (
    <Chart options={{ autoSize: true, layout: { background: { color: "transparent" }, textColor: theme === "dark" ? "#a5b4fc" : "#475569" }, grid: { vertLines: { color: theme === "dark" ? "#273449" : "#e2e8f0" }, horzLines: { color: theme === "dark" ? "#273449" : "#e2e8f0" } }, rightPriceScale: { borderColor: "transparent" }, timeScale: { borderColor: "transparent" } }} containerProps={{ className: "chart-container" }}>
      <LineSeries data={data} options={{ color: theme === "dark" ? "#a5b4fc" : "#4f46e5", lineWidth: 3 }} />
      <TimeScale><TimeScaleFitContentTrigger deps={[points]} /></TimeScale>
    </Chart>
  );
}

export default function App() {
  const [isin, setIsin] = useState("");
  const [activeIsin, setActiveIsin] = useState("");
  const [period, setPeriod] = useState<Period>("daily");
  const [result, setResult] = useState<AssetResponse | null>(null);
  const [error, setError] = useState("");
  const [validationError, setValidationError] = useState("");
  const [loading, setLoading] = useState(false);
  const [theme, setTheme] = useState<Theme>("dark");
  const requestId = useRef(0);

  useEffect(() => { document.documentElement.dataset.theme = theme; }, [theme]);

  async function loadAsset(nextIsin: string, nextPeriod: Period) {
    const currentRequest = ++requestId.current;
    setLoading(true);
    setError("");
    try {
      const response = await fetch(`${API_BASE_URL}/api/assets/${encodeURIComponent(nextIsin)}?period=${nextPeriod}`);
      const payload = await response.json() as AssetResponse & { detail?: string; message?: string };
      if (!response.ok) throw new Error(payload.detail ?? payload.message ?? "Non è stato possibile recuperare i dati.");
      if (currentRequest !== requestId.current) return;
      setResult(payload);
    } catch (requestError) {
      if (currentRequest !== requestId.current) return;
      setError(requestError instanceof Error ? requestError.message : "Si è verificato un errore. Riprova.");
    } finally {
      if (currentRequest === requestId.current) setLoading(false);
    }
  }

  function submit(event: FormEvent) {
    event.preventDefault();
    const normalized = normalizeIsin(isin);
    setIsin(normalized);
    setValidationError("");
    setError("");
    if (!normalized) return setValidationError("Inserisci un codice ISIN.");
    if (!isValidIsin(normalized)) return setValidationError("L'ISIN deve contenere 12 caratteri alfanumerici ed essere formalmente valido.");
    setActiveIsin(normalized);
    void loadAsset(normalized, period);
  }

  function changePeriod(nextPeriod: Period) {
    setPeriod(nextPeriod);
    if (activeIsin) void loadAsset(activeIsin, nextPeriod);
  }

  const latest = result?.points.at(-1);
  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand"><span className="brand-mark">AR</span><span>Asset Reader</span></div>
        <button className="theme-toggle" type="button" onClick={() => setTheme(theme === "light" ? "dark" : "light")} aria-label={`Attiva tema ${theme === "light" ? "scuro" : "chiaro"}`}>
          {theme === "light" ? "☾ Tema scuro" : "☀ Tema chiaro"}
        </button>
      </header>
      <main className="main-content">
        <section className="hero">
          <p className="eyebrow">MARKET INSIGHTS</p>
          <h1>Leggi il mercato<br /><em>con chiarezza.</em></h1>
          <p className="intro">Inserisci un ISIN per esplorare l'andamento storico di ETF e bond quotati.</p>
        </section>
        <section className="search-card" aria-label="Ricerca asset">
          <form onSubmit={submit} noValidate>
            <label htmlFor="isin">Codice ISIN</label>
            <div className="search-row">
              <input id="isin" value={isin} onChange={(event) => setIsin(event.target.value)} placeholder="Es. IE00B4L5Y983" maxLength={20} autoComplete="off" aria-invalid={Boolean(validationError)} aria-describedby={validationError ? "validation-error" : undefined} />
              <button className="primary-button" disabled={loading} type="submit">{loading ? "Ricerca..." : "Cerca asset"} <span aria-hidden="true">→</span></button>
            </div>
            {validationError && <p id="validation-error" className="inline-error" role="alert">{validationError}</p>}
          </form>
          <div className="period-picker" role="group" aria-label="Intervallo storico">
            {periods.map((option) => <button key={option.key} type="button" className={period === option.key ? "period-button active" : "period-button"} onClick={() => changePeriod(option.key)} disabled={loading && !activeIsin} aria-pressed={period === option.key}>{option.label}</button>)}
          </div>
        </section>
        <section className="result-area" aria-live="polite">
          {loading && <div className="status-card"><span className="spinner" /> Recupero dati in corso...</div>}
          {!loading && error && <div className="status-card error-card"><strong>Non possiamo mostrare questo asset.</strong><span>{error}</span><button type="button" onClick={() => activeIsin && void loadAsset(activeIsin, period)}>Riprova</button></div>}
          {!loading && !error && result && result.points.length === 0 && <div className="status-card"><strong>Nessun dato disponibile</strong><span>Il provider non ha restituito punti per questo intervallo.</span></div>}
          {!loading && !error && result && result.points.length > 0 && (
            <article className="result-card">
              <div className="result-heading"><div><p className="eyebrow">RISULTATO</p><h2>{result.name ?? result.isin}</h2><p className="muted">{result.isin}{result.currency ? ` · ${result.currency}` : ""}</p></div><div className="latest-value"><span>Ultimo valore</span><strong>{latest ? formatValue(latest.value, result.currency) : "Non disponibile"}</strong><small>{latest ? new Intl.DateTimeFormat("it-IT", { dateStyle: "medium" }).format(new Date(latest.time)) : "Data non disponibile"}</small></div></div>
              <AssetChart points={result.points} theme={theme} />
              <p className="chart-caption">Dati storici · Intervallo {periods.find((item) => item.key === result.period)?.label.toLowerCase()}</p>
            </article>
          )}
          {!loading && !error && !result && <div className="empty-state"><div className="empty-icon">⌁</div><h2>Il tuo prossimo insight<br />inizia da un ISIN.</h2><p>Il grafico e i dettagli dell'asset appariranno qui.</p></div>}
        </section>
      </main>
      <footer>Asset Reader · Dati forniti da Borsa Italiana · Non costituisce consulenza finanziaria</footer>
    </div>
  );
}
