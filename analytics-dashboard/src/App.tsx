// File: analytics-dashboard/src/App.tsx
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import { Eye, ShoppingCart, DollarSign, Activity } from 'lucide-react';

interface MetricData {
  eventType: string;
  totalCount: number;
  totalValue: number;
  lastUpdatedUtc: string;
}

export default function App() {
  const [metrics, setMetrics] = useState<Record<string, MetricData>>({});
  const [chartData, setChartData] = useState<any[]>([]);
  const [hubStatus, setHubStatus] = useState<'Connecting' | 'Connected' | 'Disconnected'>('Connecting');

  const API_BASE = 'http://localhost:5000';

  // File: analytics-dashboard/src/App.tsx

useEffect(() => {
  // 1. جلب البيانات الأولية عند تحميل الصفحة لأول مرة
  fetch(`${API_BASE}/api/analytics/live-metrics`)
    .then(res => res.json())
    .then((data: MetricData[]) => {
      const initialMetrics: Record<string, MetricData> = {};
      data.forEach(m => {
        initialMetrics[m.eventType] = m;
      });
      setMetrics(initialMetrics);
      setChartData(data.map(m => ({ name: m.eventType, Count: m.totalCount, Value: m.totalValue })));
    })
    .catch(err => console.error("❌ Failed to fetch initial metrics:", err));

  // 2. إعداد وبناء اتصال SignalR
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/analyticsHub`, {
      // إجبار المتصفح على استخدام الـ WebSockets مباشرة إذا كان مدعوماً لتخطي مشاكل الـ Negotiate التكرارية
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  let isMounted = true; // علم للتأكد من أن الكومبوننت لا يزال معروضاً

  async function startSignalR() {
    try {
      if (connection.state === signalR.HubConnectionState.Disconnected && isMounted) {
        setHubStatus('Connecting');
        await connection.start();
        if (isMounted) {
          setHubStatus('Connected');
          console.log('⚡ Connected to SignalR Hub successfully via WebSockets!');
        }
      }
    } catch (err) {
      if (isMounted) {
        setHubStatus('Disconnected');
        console.error('❌ SignalR Connection Error: ', err);
      }
    }
  }

  // الاستماع للأحداث (بالاسم الصغير الذي يرسله السيرفر كما رأينا سابقاً)
  connection.on('receivemetricsupdate', (updatedMetric: MetricData) => {
    if (!isMounted) return;
    setMetrics(prev => {
      const newMetrics = { ...prev, [updatedMetric.eventType]: updatedMetric };
      setChartData(Object.values(newMetrics).map(m => ({
        name: m.eventType,
        Count: m.totalCount,
        Value: m.totalValue
      })));
      return newMetrics;
    });
  });

  // 👈 2. أضف هذا المستمع الجديد هنا لتلقي رسالة الترحيب وتنظيف الـ Console
  connection.on('welcomemessage', (message: string) => {
    console.log('🎉 Message from Server Hub:', message);
  });
  // بدء الاتصال
  startSignalR();


  // دالة التنظيف عند إغلاق الصفحة أو إعادة تحميل الكومبوننت
  return () => {
    isMounted = false;
    if (connection.state === signalR.HubConnectionState.Connected) {
      connection.stop();
    }
  };
}, []);

  // دالة مساعدة لجلب الأرقام بأمان وتجنب الـ undefined
  const getMetric = (type: string) => metrics[type] || { totalCount: 0, totalValue: 0 };

  return (
    <div className="min-h-screen bg-gray-900 text-gray-100 p-6 font-sans">
      {/* Header */}
      <div className="flex justify-between items-center border-b border-gray-800 pb-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-white flex items-center gap-3">
            <Activity className="text-emerald-400 animate-pulse" size={32} />
            Real-Time Analytics Platform
          </h1>
          <p className="text-gray-400 mt-1">لوحة تحكم ذكية لمراقبة تدفق الأحداث والعمليات المالية بلحظتها</p>
        </div>
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-semibold bg-gray-800">
          <span className={`h-2.5 w-2.5 rounded-full ${hubStatus === 'Connected' ? 'bg-emerald-500 animate-ping' : 'bg-rose-500'}`} />
          Status: {hubStatus}
        </div>
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        {/* Page Views Card */}
        <div className="bg-gray-800 p-6 rounded-xl border border-gray-700 shadow-lg relative overflow-hidden">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">Page Views</p>
              <h3 className="text-3xl font-extrabold text-white mt-2 transition-all duration-300">
                {getMetric('PageView').totalCount.toLocaleString()}
              </h3>
            </div>
            <div className="p-3 bg-blue-500/10 text-blue-400 rounded-lg">
              <Eye size={24} />
            </div>
          </div>
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-blue-500 to-cyan-400" />
        </div>

        {/* Purchases Card */}
        <div className="bg-gray-800 p-6 rounded-xl border border-gray-700 shadow-lg relative overflow-hidden">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">Total Orders</p>
              <h3 className="text-3xl font-extrabold text-white mt-2">
                {getMetric('Purchase').totalCount.toLocaleString()}
              </h3>
            </div>
            <div className="p-3 bg-emerald-500/10 text-emerald-400 rounded-lg">
              <ShoppingCart size={24} />
            </div>
          </div>
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-emerald-500 to-teal-400" />
        </div>

        {/* Financial Value Card */}
        <div className="bg-gray-800 p-6 rounded-xl border border-gray-700 shadow-lg relative overflow-hidden">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-400 uppercase tracking-wider">Live Revenue</p>
              <h3 className="text-3xl font-extrabold text-amber-400 mt-2">
                ${getMetric('Purchase').totalValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </h3>
            </div>
            <div className="p-3 bg-amber-500/10 text-amber-400 rounded-lg">
              <DollarSign size={24} />
            </div>
          </div>
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-amber-500 to-orange-400" />
        </div>
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Events Volume Chart */}
        <div className="bg-gray-800 p-6 rounded-xl border border-gray-700">
          <h3 className="text-lg font-semibold text-white mb-4">كثافة الأحداث (Event Volume)</h3>
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="name" stroke="#9ca3af" />
                <YAxis stroke="#9ca3af" />
                <Tooltip contentStyle={{ backgroundColor: '#1f2937', borderColor: '#4b5563' }} />
                <Legend />
                <Bar dataKey="Count" fill="#3b82f6" radius={[4, 4, 0, 0]} name="عدد العمليات" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Revenue Impact Chart */}
        <div className="bg-gray-800 p-6 rounded-xl border border-gray-700">
          <h3 className="text-lg font-semibold text-white mb-4">العوائد المالية اللحظية ($)</h3>
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis dataKey="name" stroke="#9ca3af" />
                <YAxis stroke="#9ca3af" />
                <Tooltip contentStyle={{ backgroundColor: '#1f2937', borderColor: '#4b5563' }} />
                <Legend />
                <Line type="monotone" dataKey="Value" stroke="#f59e0b" strokeWidth={3} activeDot={{ r: 8 }} name="القيمة التراكمية" />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );
}