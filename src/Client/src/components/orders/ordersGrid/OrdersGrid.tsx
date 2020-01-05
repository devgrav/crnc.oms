import * as React from "react";
import { Button, Dimmer, Header, Icon, Loader, Menu, Segment, Table } from "semantic-ui-react";
import OrdersGridRow from "./OrdersGridRow";
import { inject, observer } from "mobx-react";
import OrdersGridRootStore from "./OrdersGridRootStore";
import OrdersGridStore from "./OrdersGridStore";
import { Link } from "react-router-dom";
import { nameof } from "ts-simple-nameof";
import OrdersRootStores from "../OrdersRootStores";

@inject(nameof<OrdersRootStores>(x => x.ordersGridRootStore))
@observer
export default class OrdersGrid extends React.Component<OrdersGridProps> {

    private readonly store: OrdersGridStore;

    constructor(props: OrdersGridProps){
        super(props);

        if(!props.ordersGridRootStore)
            throw Error("Not found store")

        this.store = props.ordersGridRootStore.ordersGridStore;
    }

    public async componentDidMount(): Promise<void>{
        const {store} = this;
        const orders = await this.store.getOrders();            
        store.setModel(orders);        
    }

    public render() {
        const {items} = this.store.model;
        const {isLoading} = this.store;

        return (
            <Dimmer.Dimmable dimmed={isLoading}>
                <Dimmer active={isLoading} inverted>
                    <Loader>Loading</Loader>
                </Dimmer>
                <Table celled selectable striped definition>
                    <Table.Header fullWidth>
                        <Table.Row>
                            <Table.HeaderCell colSpan="9">
                                <Button 
                                    as={Link}
                                    to="/orders/new"
                                    floated="right" 
                                    primary 
                                    content="Add order" 
                                    icon="plus"
                                />
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
                                Job Type
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Job description
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Date sent to customer
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Customer sighnoff
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Status
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {items.map((o) =>
                            <OrdersGridRow key={o.id} item={o}/>)}
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

interface OrdersGridProps{
    ordersGridRootStore?: OrdersGridRootStore;
}
