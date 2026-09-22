import axios from "axios";
import { getStoredToken } from "@/auth/tokenStorage";

// Один настроенный клиент на всё приложение. Адрес бэкенда не хардкодится:
// пути относительные, а разводит их по сервисам nginx (прод) или dev-прокси Vite.
const apiClient = axios.create({
    baseURL: "/api",
    timeout: 30_000,
    headers: { "Content-Type": "application/json" },
});

// Токен читается на каждом запросе, а не запекается в инстанс при создании:
// старый AxiosProxy клал его в defaults.headers при первом обращении и поэтому
// требовал ручного clear() при каждой смене пользователя.
apiClient.interceptors.request.use((config) => {
    const token = getStoredToken();

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

export default apiClient;
