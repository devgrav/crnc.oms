import * as React from "react";
import { Link } from "react-router-dom";
import { Checkbox, Icon, Label, Table } from "semantic-ui-react";
import {UserItemDto, UserService} from "../../services/UserService";

const UsersGrid: React.StatelessComponent<UsersGridRowProps> = (props) => {
        return (
                <Table.Row>
                    <Table.Cell>
                        <Label color="blue" as={Link} to={`/users/${props.userItem.id}`}>
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
    userItem: UserItemDto;
}
