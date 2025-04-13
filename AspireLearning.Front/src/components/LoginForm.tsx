import { useState } from "react";
import { FiMail, FiLock } from "react-icons/fi";
import { useNavigate } from "react-router-dom";
import {useAuth} from "../context/AuthContext.tsx";

export default function LoginForm() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError("");

        try {
            const res = await fetch("/api/identity/auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email, password }),
            });

            if (!res.ok) throw new Error("Giriş başarısız");

            const data = await res.json();
            login(data.token, data.user);
            navigate("/dashboard");
        } catch (err: any) {
            setError(err.message || "Hata oluştu");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="w-[445px] h-[620px] bg-background p-[30px] rounded-[30px] border-[3px] border-primary shadow-[0px_4px_12px_5px_rgba(0,0,0,0.1)] flex flex-col justify-center">

            <div className="flex flex-col mb-15">
                <h2 className="text-2xl font-family-poppins-regular text-gray-333 ml-4">Login</h2>
                <div className="w-1/2 h-[2px] bg-gray-666 mt-1" />
            </div>

            <form className="flex flex-col gap-8" onSubmit={handleSubmit}>
                {/* Email */}
                <div className="flex items-center border border-primary rounded px-3 py-2 bg-white">
                    <FiMail className="text-gray-666 mr-2" />
                    <input
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full bg-transparent outline-none text-sm text-gray-800"
                        required
                    />
                </div>

                {/* Password */}
                <div className="flex items-center border border-primary rounded px-3 py-2 bg-white">
                    <FiLock className="text-gray-666 mr-2" />
                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full bg-transparent outline-none text-sm text-gray-800"
                        required
                    />
                </div>

                {/* Error Message */}
                {error && <p className="text-sm text-red-500 text-center">{error}</p>}

                {/* Forgot */}
                <div>
                    <a
                        href="#"
                        className="text-sm text-primary underline hover:opacity-80 transition"
                    >
                        Şifremi Unuttum
                    </a>
                </div>

                {/* Submit Button */}
                <button
                    type="submit"
                    disabled={loading}
                    className="self-end w-1/3 mt-2 bg-primary text-white py-2 rounded-full hover:brightness-90 transition disabled:opacity-60"
                >
                    {loading ? "Giriş Yapılıyor..." : "Login →"}
                </button>
            </form>
        </div>
    );
}
