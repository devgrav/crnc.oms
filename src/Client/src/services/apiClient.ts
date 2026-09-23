import axios from "axios";
import { clearStoredUser, getStoredToken } from "@/auth/tokenStorage";

const apiClient = axios.create({
    baseURL: "/api",
    timeout: 30_000,
    headers: { "Content-Type": "application/json" },
});

// Токен читается на каждом запросе, а не запекается в инстанс при создании.
apiClient.interceptors.request.use((config) => {
    const token = getStoredToken();

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

// Протухшая сессия не должна оставлять пользователя на экране с сообщением: токен
// выбрасывается, и приложение уходит на /login. Проверка токена не даёт зациклиться
// на 401 от неаутентифицированных запросов.
apiClient.interceptors.response.use(undefined, (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401 && getStoredToken()) {
        clearStoredUser();
        window.location.assign("/login");
    }

    throw error;
});

export default apiClient;
