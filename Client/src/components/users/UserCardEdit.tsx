import * as React from "react";
import { Button, ButtonProps, Card, Divider, Form, Grid, Image, InputOnChangeData, Modal, ModalProps } from "semantic-ui-react";
import { UserItemDto, UserService } from "../../services/UserService";

export default class UserCardEdit extends React.Component<UserCardEditProps, UserCardEditState>{

    constructor(props: UserCardEditProps){
        super(props);

        this.state = {
            user: props.user,
            isLoading: false
        };

        this.onClose = this.onClose.bind(this);
        this.onChange = this.onChange.bind(this);
        this.onCancel = this.onCancel.bind(this);
        this.onSave = this.onSave.bind(this);
    }

    private showLoader(){
        this.setState({
            isLoading: true
        });
    }

    private hideLoader(){
        this.setState({
            isLoading: false
        });
    }

    private onClose(event: React.MouseEvent<HTMLElement>, data: ModalProps): void{
        this.props.onCancelEdit();
    }

    private onCancel(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps): void{
        this.props.onCancelEdit();
    }

    private async onSave(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps): Promise<void>{
       try{
           this.showLoader();
           await UserService.putUser(this.state.user);
           this.hideLoader();
           this.props.onCancelEdit();
           this.props.onSaved();
       }catch (error){
            this.hideLoader();
        }
    }

    private onChange(event: any, data: any): void{
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;
        const user = {...this.state.user, ...{[name]: value}};
        this.setState({
            user
        });
    }

    public render(){
        return (
            <Modal open={true} closeIcon onClose={this.onClose}>
                <Modal.Header>Edit user</Modal.Header>
                <Modal.Content image>
                    <Form id="userForm" loading={this.state.isLoading} className="ui form" onSubmit={this.onSave}>
                        <Grid columns={2}>
                            <Grid.Column>
                                <Image
                                    size="medium"
                                    src={`data:${this.state.user.photoMimeType};base64,
                                        ${this.state.user.photoBase64}`}
                                />
                            </Grid.Column>
                            <Grid.Column>
                                <Form.Group>
                                    <Form.Input
                                        name="login"
                                        onChange={this.onChange}
                                        label="Login"
                                        value={this.state.user.login}
                                    />
                                    <Form.Input
                                        name="password"
                                        onChange={this.onChange}
                                        label="Password"
                                        type="password"
                                        value={this.state.user.password}
                                    />
                                </Form.Group>
                                <Form.Group>
                                    <Form.Input
                                        name="firstName"
                                        onChange={this.onChange}
                                        label="First name"
                                        value={this.state.user.firstName}
                                    />
                                    <Form.Input
                                        name="lastName"
                                        onChange={this.onChange}
                                        label="Last name"
                                        value={this.state.user.lastName}
                                    />
                                </Form.Group>
                                <Form.Group>
                                    <Form.Input
                                        name="email"
                                        onChange={this.onChange}
                                        label="Email"
                                        type="email"
                                        value={this.state.user.email}
                                    />
                                    <Form.Input
                                        name="phone"
                                        onChange={this.onChange}
                                        label="Phone"
                                        value={this.state.user.phone || ""}
                                    />
                                </Form.Group>
                                <Form.Checkbox
                                    name="isActive"
                                    onChange={this.onChange}
                                    label="Active"
                                    checked={this.state.user.isActive}
                                />
                            </Grid.Column>
                        </Grid>
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button basic color="green" type="submit" content="Save" form="userForm"/>
                    <Button basic color="red" type="reset" content="Cancel" onClick={this.onCancel}/>
                </Modal.Actions>
            </Modal>
        );
    }
}

interface UserCardEditProps{
    user: UserItemDto;
    onCancelEdit(): void;
    onSaved(): void;
}

interface UserCardEditState{
    user: UserItemDto;
    isLoading: boolean;
}
