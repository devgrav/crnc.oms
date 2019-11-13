import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table, Item } from "semantic-ui-react";
import { OrdersForGridItemDto } from "../../services/OrderService";
import {UserItemDto, UserService} from "../../services/UserService";

const OrdersGridRow: React.StatelessComponent<OrdersGridRowProps> = (props) => {
        const {item} = props;
        return (
                <Table.Row>
                    <Table.Cell>
                        <Label color="blue">
                            <Icon name="pencil"/>
                            Edit
                        </Label>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.number}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.createdDate}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.customer}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.jobDescription}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.dateSentToCustomer}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.customerSignOffType}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.status}</div>
                    </Table.Cell>
                </Table.Row>
        );
};

export default OrdersGridRow;

interface OrdersGridRowProps{
    item: OrdersForGridItemDto;
}
