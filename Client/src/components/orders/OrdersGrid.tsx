import * as React from "react";
import { Button, Dimmer, Header, Icon, Loader, Menu, Segment, Table } from "semantic-ui-react";
import { OrderItemDto } from "../../services/OrderService";
import {UserItemDto, UserService} from "../../services/UserService";
import OrdersGridRow from "./OrdersGridRow";

export default class OrdersGrid extends React.Component<{}, OrdersGridState> {
    constructor(props: any){
        super(props);

        this.state = {
            orders: [],
            isLoading: false
        };
    }

    public componentDidMount(): void{
    }

    public showLoading(): void{
        this.setState({
            ...this.state, isLoading: true
        });
    }

    public hideLoading(): void{
        this.setState({
            ...this.state, isLoading: false
        });
    }

    public render() {
        return (
            <Dimmer.Dimmable dimmed={this.state.isLoading}>
                <Dimmer active={this.state.isLoading} inverted>
                    <Loader>Loading</Loader>
                </Dimmer>
                <Table celled selectable striped definition>
                    <Table.Header fullWidth>
                        <Table.Row>
                            <Table.HeaderCell colSpan="9">
                                <Button floated="right" primary content="Add estimate" icon="plus"/>
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>
                    <Table.Header fullWidth>
                        <Table.Row>
                            <Table.HeaderCell width={1}/>
                            <Table.HeaderCell>
                                Order #
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Date Created
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Customer
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Type
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Work description
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Date sent to customer
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Customer sighnoff
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Order status
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {this.state.orders.map((est) =>
                            <OrdersGridRow key={est.id} estimateItem={est}/>)}
                    </Table.Body>
                    <Table.Footer fullWidth>
                        <Table.Row>
                            <Table.HeaderCell colSpan="9">
                                <Menu floated="right" pagination>
                                    <Menu.Item as="a" icon>
                                        <Icon name="chevron left" />
                                    </Menu.Item>
                                    <Menu.Item as="a">1</Menu.Item>
                                    <Menu.Item as="a">2</Menu.Item>
                                    <Menu.Item as="a">3</Menu.Item>
                                    <Menu.Item as="a">4</Menu.Item>
                                    <Menu.Item as="a" icon>
                                        <Icon name="chevron right" />
                                    </Menu.Item>
                                </Menu>
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Footer>
                </Table>
            </Dimmer.Dimmable>
        );
    }
}

interface OrdersGridState{
    orders: OrderItemDto[];
    isLoading: boolean;
}
