import { Alert, Badge, Button, Group, LoadingOverlay, Table } from "@mantine/core";
import { Link, Outlet } from "react-router";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { getOrders } from "@/services/orders.service";
import { OrderStatus, type OrderRow } from "@/types/orders.types";

// Layout-роут: карточка заказа (/orders/new, /orders/:id) рендерится через Outlet
// поверх этого же списка, а не вместо него.
export default function OrdersPage() {
    const { data: orders = [], isLoading, error } = useServiceQuery(["orders"], getOrders);

    return (
        <div style={{ position: "relative" }}>
            <LoadingOverlay visible={isLoading} />
            {error && <Alert color="red" mb="sm">{error.message}</Alert>}
            <Group justify="flex-end" mb="sm">
                <Button component={Link} to="/orders/new" data-testid="orders-add">
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
                                    ✎
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
        </div>
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
