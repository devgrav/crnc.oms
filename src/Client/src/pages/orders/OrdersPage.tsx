import { Alert, Badge, Box, Button, Group, LoadingOverlay, Table } from "@mantine/core";
import { IconPencil, IconPlus } from "@tabler/icons-react";
import { Link, Outlet } from "react-router";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { getOrders } from "@/services/orders.service";
import { OrderStatus, type OrderRow } from "@/types/orders.types";

export default function OrdersPage() {
    const { data: orders = [], isLoading, error } = useServiceQuery(["orders"], getOrders);

    return (
        <Box pos="relative">
            <LoadingOverlay visible={isLoading} />
            {error && <Alert color="red" mb="sm">{error.message}</Alert>}
            <Group justify="flex-end" mb="sm">
                <Button component={Link} to="/orders/new" leftSection={<IconPlus size={16} />} data-testid="orders-add">
                    Add order
                </Button>
            </Group>
            <Table striped highlightOnHover withTableBorder data-testid="orders-grid">
                <Table.Thead>
                    <Table.Tr>
                        <Table.Th w={60} />
                        <Table.Th>Order #</Table.Th>
                        <Table.Th>Date Created</Table.Th>
                        <Table.Th>Customer</Table.Th>
                        <Table.Th>Job Type</Table.Th>
                        <Table.Th>Job description</Table.Th>
                        <Table.Th>Date sent to customer</Table.Th>
                        <Table.Th>Customer signoff</Table.Th>
                        <Table.Th>Status</Table.Th>
                    </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                    {orders.map((order) => (
                        <Table.Tr key={order.id} data-testid="order-row">
                            <Table.Td>
                                <Button
                                    component={Link}
                                    to={`/orders/${order.id}`}
                                    size="compact-sm"
                                    variant="light"
                                    data-testid="order-edit"
                                    aria-label={`Edit order ${order.number}`}
                                >
                                    <IconPencil size={14} />
                                </Button>
                            </Table.Td>
                            <Table.Td data-testid="order-number">{order.number}</Table.Td>
                            <Table.Td>{order.createdDate}</Table.Td>
                            <Table.Td>{order.customer}</Table.Td>
                            <Table.Td>{order.jobType}</Table.Td>
                            <Table.Td data-testid="order-description">{order.jobDescription}</Table.Td>
                            <Table.Td>{order.dateSentToCustomer}</Table.Td>
                            <Table.Td>{order.customerSignOffType}</Table.Td>
                            <Table.Td>
                                <Badge color={statusColor(order.statusEnum)} data-testid="order-status">
                                    {order.status}
                                </Badge>
                            </Table.Td>
                        </Table.Tr>
                    ))}
                </Table.Tbody>
            </Table>
            <Outlet />
        </Box>
    );
}

function statusColor(status: OrderRow["statusEnum"]): string {
    switch (status) {
        case OrderStatus.ConvertedToJob:
            return "green";
        case OrderStatus.Closed:
            return "red";
        default:
            return "blue";
    }
}
