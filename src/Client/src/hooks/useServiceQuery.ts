import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import type { ServiceResult } from "@/services/result";

export interface ServiceQueryOptions {
    // gcTime: 0 отключает кэш между монтированиями. Нужен карточкам: форма сеет
    // состояние один раз при монтировании, и подсунутый из кэша устаревший ответ
    // остался бы в ней навсегда - сохранение ушло бы со старыми значениями.
    gcTime?: number;
    staleTime?: number;
}

// Сервисы не бросают исключения, поэтому неуспех разворачивается в throw для React Query.
export function useServiceQuery<T>(
    queryKey: unknown[],
    fetcher: () => Promise<ServiceResult<T>>,
    options: ServiceQueryOptions = {},
): UseQueryResult<T, Error> {
    return useQuery<T, Error>({
        ...options,
        queryKey,
        queryFn: async () => {
            const result = await fetcher();

            if (!result.success || result.data === undefined) {
                throw new Error(result.generalError ?? "Request failed");
            }

            return result.data;
        },
    });
}
