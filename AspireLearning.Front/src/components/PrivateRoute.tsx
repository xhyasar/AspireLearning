import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const PrivateRoute = () => {
    const { isAuthenticated, isLoading } = useAuth();

    // Auth bilgisi henüz yüklenmediyse boş sayfa ya da loader döndür
    if (isLoading) {
        return <div>Yükleniyor...</div>; // Ya da spinner component vs.
    }

    return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
};

export default PrivateRoute;
