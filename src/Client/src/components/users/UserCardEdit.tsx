import * as React from "react";
import { Button, ButtonProps, Card, Divider, Form, Grid, Image, InputOnChangeData, Message, Modal, ModalProps, Segment, FormProps } from "semantic-ui-react";
import * as noAvatar from "../../assets/images/noavatar.png";
import { UserItemDto, UserService } from "../../services/UserService";
import { Guid } from "guid-typescript";

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
        this.onFileUploadClick = this.onFileUploadClick.bind(this);
        this.onFileSelected = this.onFileSelected.bind(this);
    }

    private fileInput: HTMLInputElement | null = null;

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

    private onFileUploadClick(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps): void{
        if (this.fileInput){
            this.fileInput.click();
        }
    }

    private onFileSelected(event: any): void{
        const files: FileList = event.nativeEvent.target.files;
        if (files && files.length > 0){
            const file = files[0];
            const fileReader = new FileReader();
            fileReader.onload = (e: any) => {
                const binaryString = fileReader.result;
                const base64StringPhoto = btoa(binaryString as string);
                const user = {...this.state.user, ...{photoBase64: base64StringPhoto, photoMimeType: file.type}};
                this.setState({
                    user
                });
            };

            fileReader.readAsBinaryString(file);
        }
    }

    private async onSave(event: React.FormEvent<HTMLElement>, data: FormProps): Promise<void>{
       try{
           this.showLoader();
           if (this.state.user.id === Guid.EMPTY){
                await UserService.postUser(this.state.user);
           }
           else{
                await UserService.putUser(this.state.user);
           }
           this.hideLoader();
           this.props.onCancelEdit();
           this.props.onSaved();
       }catch (error){
            this.hideLoader();
            if (error.response){
                if (error.response.status === 400){
                    this.setState({
                        validationInfo: error.response.data
                    });
                }
            }
        }
    }

    private onChange(event: any, data: any): void{
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;
        const user = {...this.state.user, ...{[name]: value}};

        if (this.state.validationInfo && this.state.validationInfo[name]){
            let validationInfo = {...this.state.validationInfo};
            delete validationInfo[name];
            if (Object.keys(validationInfo).length === 0){
                validationInfo = undefined;
            }

            this.setState({
                validationInfo
            });
        }

        this.setState({
            user
        });
    }

    private getValidationMessages(): string[]{
        if (this.state.validationInfo){
            let messages: string[] = [];
            Object.keys(this.state.validationInfo).forEach((key) => {
                messages = messages.concat(this.state.validationInfo[key]);
              });

            return messages;
        }

        return [];
    }

    private hasFieldValidationError(name: string){
        if (this.state.validationInfo && this.state.validationInfo[name]){
            return true;
        }

        return false;
    }

    public render(){
        return (
            <Modal open={true} closeIcon onClose={this.onClose}>
                <Modal.Header>{this.props.user.id === Guid.EMPTY ? "Add new user" : "Edit user"}</Modal.Header>
                <Modal.Content as={Segment} basic clearing loading={this.state.isLoading}>
                    {this.state.validationInfo && <Message
                        error
                        header="There was some errors with your submission"
                        list={this.getValidationMessages()}
                    />}
                    <Form id="userForm" className="ui form" onSubmit={this.onSave}>
                        <Grid columns={2}>
                        <Grid.Row>
                            <Grid.Column width={5}>
                                <Image
                                    bordered
                                    fluid
                                    src={this.state.user.photoBase64 === null
                                        || this.state.user.photoBase64 === undefined
                                        ? noAvatar
                                        : `data:${this.state.user.photoMimeType};base64,
                                        ${this.state.user.photoBase64}`}
                                />
                                <Button
                                    attached="top"
                                    icon="upload"
                                    content="Upload photo"
                                    onClick={this.onFileUploadClick}
                                />
                                <input
                                    ref={(input) => this.fileInput = input}
                                    style={{display: "none"}}
                                    onChange={this.onFileSelected}
                                    id="uploadPhoto"
                                    type="file"
                                    accept="image/*"
                                />
                            </Grid.Column>
                            <Grid.Column  width={11}>
                                <Form.Group widths="equal">
                                    <Form.Input
                                        name="login"
                                        className="required"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("login")}
                                        label="Login"
                                        value={this.state.user.login || ""}
                                        autoComplete="off"
                                    />
                                    <Form.Input
                                        name="password"
                                        className="required"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("password")}
                                        label="Password"
                                        type="password"
                                        value={this.state.user.password || ""}
                                        autoComplete="off"
                                    />
                                </Form.Group>
                                <Form.Group widths="equal">
                                    <Form.Input
                                        name="firstName"
                                        className="required"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("firstName")}
                                        label="First name"
                                        value={this.state.user.firstName || ""}
                                        autoComplete="off"
                                    />
                                    <Form.Input
                                        name="lastName"
                                        className="required"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("lastName")}
                                        label="Last name"
                                        value={this.state.user.lastName || ""}
                                        autoComplete="off"
                                    />
                                </Form.Group>
                                <Form.Group widths="equal">
                                    <Form.Input
                                        name="email"
                                        className="required"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("email")}
                                        label="Email"
                                        type="email"
                                        value={this.state.user.email || ""}
                                        autoComplete="off"
                                    />
                                    <Form.Input
                                        name="phone"
                                        onChange={this.onChange}
                                        error={this.hasFieldValidationError("phone")}
                                        label="Phone"
                                        value={this.state.user.phone || ""}
                                        autoComplete="off"
                                    />
                                </Form.Group>
                                <Form.Checkbox
                                    name="isActive"
                                    onChange={this.onChange}
                                    label="Active"
                                    checked={this.state.user.isActive}
                                    disabled={this.props.user.id === Guid.EMPTY}
                                />
                            </Grid.Column>
                            </Grid.Row>
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
    validationInfo?: any;
    isLoading: boolean;
}
