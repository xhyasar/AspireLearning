import { Routes, Route } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import WarehousePage from './pages/WarehousePage';
import ManagementPage from './pages/ManagementPage';
import ProductsPage from './pages/ProductsPage';
import PrivateRoute from './components/PrivateRoute';
import Layout from "./layout.tsx";

function App() {
    return (
        <Routes>
            <Route path="/login" element={<LoginPage />} />

            {/* Protected routes wrapped in Layout */}
            <Route element={<PrivateRoute />}>
                <Route element={<Layout />}>
                    <Route path="/dashboard" element={<DashboardPage />} />
                    <Route path="/warehouse" element={<WarehousePage />} />
                    <Route path="/products" element={<ProductsPage />} />
                    <Route path="/management" element={<ManagementPage />} />
                </Route>
            </Route>
        </Routes>
    );
}

export default App;
