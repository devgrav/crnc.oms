export interface JobRow {
    id: string;
    number: string;
    dateCreated: string;
    manager: string;
    jobType: string;
    jobDescription: string;
    materialSource: string;
    priority: string;
    priorityEnum: Priority;
    // Опечатка бэкенда: поле действительно называется isJobCompeted.
    isJobCompeted: boolean;
}

export const Priority = {
    High: 1,
    Middle: 2,
    Low: 3,
} as const;

export type Priority = (typeof Priority)[keyof typeof Priority];
