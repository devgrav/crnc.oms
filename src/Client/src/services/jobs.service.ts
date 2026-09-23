import apiClient from "./apiClient";
import { requestItems } from "./request";
import type { ServiceResult } from "./result";
import type { ItemsResponse } from "@/types/api.types";
import type { JobRow } from "@/types/jobs.types";

export function getJobs(): Promise<ServiceResult<JobRow[]>> {
    return requestItems(() => apiClient.get<ItemsResponse<JobRow>>("/production/jobs"));
}
