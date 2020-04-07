import * as React from "react";
import { Button, Dimmer, Header, Icon, Loader, Menu, Segment, Table } from "semantic-ui-react";
import OrdersGridRow from "./JobsGridRow";
import { inject, observer } from "mobx-react";
import OrdersGridRootStore from "./JobsGridRootStore";
import OrdersGridStore from "./JobsGridStore";
import { Link } from "react-router-dom";
import { nameof } from "ts-simple-nameof";
import JobsGridRootStore from "./JobsGridRootStore";
import JobsGridStore from "./JobsGridStore";
import JobsRootStores from "../JobsRootStores";

@inject(nameof<JobsRootStores>(x => x.jobsGridRootStore))
@observer
export default class JobsGrid extends React.Component<JobsGridProps> {

    private readonly store: JobsGridStore;

    constructor(props: JobsGridProps){
        super(props);

        if(!props.jobsGridRootStore)
            throw Error("Not found store")

        this.store = props.jobsGridRootStore.jobsGridStore;
    }

    public async componentDidMount(): Promise<void>{
        const {store} = this;
        const orders = await this.store.getJobs();            
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
                            <Table.HeaderCell width={1}/>
                            <Table.HeaderCell>
                                Job #
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Date Created
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Manager
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Job Type
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Job description
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Material source
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Priority
                            </Table.HeaderCell>
                            <Table.HeaderCell>
                                Job completed
                            </Table.HeaderCell>
                        </Table.Row>
                    </Table.Header>
                    <Table.Body>
                        {items.map((o) =>
                            <OrdersGridRow key={o.id} item={o}/>)}
                    </Table.Body>
                </Table>
            </Dimmer.Dimmable>
        );
    }
}

interface JobsGridProps{
    jobsGridRootStore?: JobsGridRootStore;
}
