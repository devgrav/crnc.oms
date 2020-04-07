import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table, Item, Button, SemanticCOLORS } from "semantic-ui-react";
import OrderGridRowModel from "./JobsGridModelRow";
import JobsGridRowModel from "./JobsGridModelRow";
import { Priority } from "../priority";

const JobsGridRow: React.StatelessComponent<JobsGridRowProps> = (props) => {
        const {item} = props;

        let color: SemanticCOLORS = "blue";
        if(item.priorityEnum === Priority.High)
            color = "red";
        if(item.priorityEnum === Priority.Middle)
            color = "blue";
        if(item.priorityEnum === Priority.Low)
            color = "grey";

        return (
                <Table.Row>
                    <Table.Cell>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.number}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.dateCreated}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.manager}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.jobType}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.jobDescription}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.materialSource}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <Label color={color}>{item.priority}</Label>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{item.isJobCompeted ? <Label color="green">Yes</Label> : <Label color="blue">No</Label>}</div>
                    </Table.Cell>
                </Table.Row>
        );
};

export default JobsGridRow;

interface JobsGridRowProps{
    item: JobsGridRowModel;
}
