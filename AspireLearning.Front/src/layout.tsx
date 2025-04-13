// components/Layout.tsx
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import {useAuth} from "./context/AuthContext.tsx";
import Logo from "./components/Logo.tsx";

const menuItems = [
    { name: "Dashboard", path: "/dashboard" },
    { name: "Warehouses", path: "/warehouse" },
    { name: "Products", path: "/products" },
    { name: "Management", path: "/management" },
];

export default function Layout() {
    const { logout, user } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <div className="flex h-screen bg-primary">
            {/* Sidebar */}
            <div className="w-[220px]  bg-gradient-to-b from-primary from-10% to-primary-dark text-white flex flex-col justify-between py-6 px-4">
                <div>
                    <Logo />
                    {menuItems.map(({ name, path }) => (
                        <NavLink
                            key={path}
                            to={path}
                            className={({ isActive }) =>
                                `block px-4 py-2 rounded mb-2 text-left font-medium ${
                                    isActive ? "bg-primary-dark" : "hover:bg-primary text-gray-333 bg-background"
                                }`
                            }
                        >
                            {name}
                        </NavLink>
                    ))}
                </div>

                <button
                    onClick={handleLogout}
                    className="block px-4 py-2 rounded mb-2 text-left font-medium text-gray-333 bg-background hover:bg-red-500"
                >
                    Logout
                </button>
            </div>

            {/* Right content */}
            <div className="flex-1 flex flex-col bg-primary rounded-tl-[30px] overflow-hidden">
                {/* Header */}
                <div className="px-10 py-6 border-b border-gray-200 flex justify-between items-center">
                    <h1 className="text-xl font-semibold text-gray-700">
                        {user?.companyName}
                    </h1>
                    <img
                        src={user?.pictureUrl || "https://via.placeholder.com/150"}
                        alt="User"
                        className="w-10 h-10 rounded-full border"
                    />
                </div>

                {/* Page content */}
                <div className="flex-1 overflow-y-auto px-10 py-6 bg-white">
                    <Outlet />
                </div>
            </div>
        </div>
    );
}
