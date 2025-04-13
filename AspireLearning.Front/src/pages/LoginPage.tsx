import Logo from "../components/Logo";
import LoginForm from "../components/LoginForm";

export default function LoginPage() {
    return (
        <div className="min-h-screen flex bg-gradient-to-b from-primary to-primary-dark">
            {/* Sol panel */}
            <div className="flex-1 flex flex-col items-center justify-center">
                <Logo />
                <p className="text-sm opacity-70 text-gray-333">"Smart warehouse. Smooth operations."</p>
            </div>

            {/* Sağ panel */}
            <div className="flex-1 flex items-center justify-center">
                <LoginForm />
            </div>
        </div>
    );
}
