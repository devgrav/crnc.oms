import * as React from "react";
import { Button, Dimmer, Header, Icon, Loader, Menu, Segment, Table } from "semantic-ui-react";
import { EstimateItemDto } from "../../services/EstimateService";
import {UserItemDto, UserService} from "../../services/UserService";
import EstimatesGridRow from "./EstimatesGridRow";

export default class EstimatesGrid extends React.Component<{}, EstimatesGridState> {
    constructor(props: any){
        super(props);

        this.state = {
            estimates: [],
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
                                Estimate #
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
                                Estimate status
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {this.state.estimates.map((est) =>
                            <EstimatesGridRow key={est.id} estimateItem={est}/>)}
                    </Table.Body>
                    <Table.Footer fullWidth>
                        <Table.Row>
                            <Table.HeaderCell colSpan="9">
                                <Menu floated="right" pagination>
                                    <Menu.Item as="a" icon>
                                        <Icon name="left chevron" />
                                    </Menu.Item>
                                    <Menu.Item as="a">1</Menu.Item>
                                    <Menu.Item as="a">2</Menu.Item>
                                    <Menu.Item as="a">3</Menu.Item>
                                    <Menu.Item as="a">4</Menu.Item>
                                    <Menu.Item as="a" icon>
                                        <Icon name="right chevron" />
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

interface EstimatesGridState{
    estimates: EstimateItemDto[];
    isLoading: boolean;
}
