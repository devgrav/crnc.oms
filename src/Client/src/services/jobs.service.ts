import apiClient from "./apiClient";
import { toFailure } from "./errorHandler";
import { ok, type ServiceResult } from "./result";
import type { ItemsResponse } from "@/types/api.types";
import type { JobRow } from "@/types/jobs.types";

export async function getJobs(): Promise<ServiceResult<JobRow[]>> {
    try {
        const response = await apiClient.get<ItemsResponse<JobRow>>("/production/jobs");
        return ok(response.data.items ?? []);
    } catch (error) {
        return toFailure(error);
    }
}
