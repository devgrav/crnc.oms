import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table } from "semantic-ui-react";
import { OrderItemDto } from "../../services/OrderService";
import {UserItemDto, UserService} from "../../services/UserService";

const OrdersGridRow: React.StatelessComponent<OrdersGridRowProps> = (props) => {
        return (
                <Table.Row>
                    <Table.Cell>
                        <Label color="blue" as={Link}>
                            <Icon name="pencil"/>
                            Edit
                        </Label>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{""}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{}</div>
                    </Table.Cell>
                </Table.Row>
        );
};

export default OrdersGridRow;

interface OrdersGridRowProps{
    estimateItem: OrderItemDto;
}
