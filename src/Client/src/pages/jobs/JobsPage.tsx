import { Alert, Badge, LoadingOverlay, Table } from "@mantine/core";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { getJobs } from "@/services/jobs.service";
import { Priority, type JobRow } from "@/types/jobs.types";

export default function JobsPage() {
    const { data: jobs = [], isLoading, error } = useServiceQuery(["jobs"], getJobs);

    if (error) {
        return <Alert color="red">{error.message}</Alert>;
    }

    return (
        <div style={{ position: "relative" }}>
            <LoadingOverlay visible={isLoading} />
            <Table striped highlightOnHover withTableBorder data-testid="jobs-grid">
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th>Job #</Table.Th>
                        <Table.Th>Date Created</Table.Th>
                        <Table.Th>Manager</Table.Th>
                        <Table.Th>Job Type</Table.Th>
                        <Table.Th>Job description</Table.Th>
                        <Table.Th>Material source</Table.Th>
                        <Table.Th>Priority</Table.Th>
                        <Table.Th>Job completed</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {jobs.map((job) => (
                        <Table.Tr key={job.id} data-testid="job-row">
                            <Table.Td data-testid="job-number">{job.number}</Table.Td>
                            <Table.Td>{job.dateCreated}</Table.Td>
                            <Table.Td>{job.manager}</Table.Td>
                            <Table.Td>{job.jobType}</Table.Td>
                            <Table.Td>{job.jobDescription}</Table.Td>
                            <Table.Td>{job.materialSource}</Table.Td>
                            <Table.Td>
                                <Badge color={priorityColor(job.priorityEnum)}>{job.priority}</Badge>
                            </Table.Td>
                            <Table.Td>
                                <Badge color={job.isJobCompeted ? "green" : "blue"}>
                                    {job.isJobCompeted ? "Yes" : "No"}
                                </Badge>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
        </div>
    );
}

function priorityColor(priority: JobRow["priorityEnum"]): string {
    switch (priority) {
        case Priority.High:
            return "red";
        case Priority.Middle:
            return "blue";
        default:
            return "gray";
    }
}
