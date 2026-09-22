import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table, Item, Button, SemanticCOLORS } from "semantic-ui-react";
import OrderGridRowModel from "./OrderGridModelRow";
import { OrderStatus } from "../OrderStatus";

const OrdersGridRow: React.StatelessComponent<OrdersGridRowProps> = (props) => {
        const {item} = props;

        let color: SemanticCOLORS = "blue";
        if(item.statusEnum === OrderStatus.ConvertedToJob)
            color = "green";
        if(item.statusEnum === OrderStatus.Closed)
            color = "red";

        return (
                <Table.Row data-testid="order-row" data-order-number={item.number}>
                    <Table.Cell>
                    <Button
                        data-testid="order-edit"
                        as={Link}
                        to={`/orders/${item.id}`}
                        primary
                        icon="pencil"
                        size="tiny"
                    />
                    </Table.Cell>
                    <Table.Cell>
                        <div data-testid="order-number">{item.number}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.createdDate}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.customer}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.jobType}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div data-testid="order-description">{item.jobDescription}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.dateSentToCustomer}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.customerSignOffType}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <Label data-testid="order-status" color={color}>{item.status}</Label>
                    </Table.Cell>
                </Table.Row>
        );
};

export default OrdersGridRow;

interface OrdersGridRowProps{
    item: OrderGridRowModel;
}
