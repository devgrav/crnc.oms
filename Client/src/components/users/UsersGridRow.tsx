import * as React from "react";
import { Checkbox, Icon, Label, Table } from "semantic-ui-react";
import {UserGridItem, UserService} from "../../services/UserService";

const UsersGrid: React.StatelessComponent<UsersGridRowProps> = (props) => {
        return (
                <Table.Row>
                    <Table.Cell>
                        <Label as="a" color="blue">
                            <Icon name="pencil"/>
                            Edit
                        </Label>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{""}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{props.userItem.fullName}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{props.userItem.login}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <div>{props.userItem.email}</div>
                    </Table.Cell>
                    <Table.Cell>
                        <Checkbox slider checked={props.userItem.isActive} disabled/>
                    </Table.Cell>
                </Table.Row>
        );
};

export default UsersGrid;

interface UsersGridRowProps{
    userItem: UserGridItem;
}
