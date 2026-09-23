import { useQuery, type UseQueryResult } from "@tanstack/react-query";
import type { ServiceResult } from "@/services/result";

// Сервисы не бросают исключения, поэтому неуспех разворачивается в throw для React Query.
export function useServiceQuery<T>(
    queryKey: unknown[],
    fetcher: () => Promise<ServiceResult<T>>,
): UseQueryResult<T, Error> {
    return useQuery<T, Error>({
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
