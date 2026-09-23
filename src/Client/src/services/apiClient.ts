import axios from "axios";
import { getStoredToken } from "@/auth/tokenStorage";

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
