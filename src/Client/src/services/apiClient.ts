import axios from "axios";
import { getStoredToken } from "@/auth/tokenStorage";

// Адрес бэкенда не хардкодится: пути относительные, разводит их nginx или dev-прокси.
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

export default apiClient;
