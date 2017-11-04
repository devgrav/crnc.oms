import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table } from "semantic-ui-react";
import { EstimateItemDto } from "../../services/EstimateService";
import {UserItemDto, UserService} from "../../services/UserService";

const EstimatesGridRow: React.StatelessComponent<EstimatesGridRowProps> = (props) => {
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

export default EstimatesGridRow;

interface EstimatesGridRowProps{
    estimateItem: EstimateItemDto;
}
