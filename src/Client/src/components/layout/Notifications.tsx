import * as React from 'react';
import * as signalR from "@microsoft/signalr";
import { Popup, Icon, Message, Label } from 'semantic-ui-react';
import APP_CONFIG from '../../config';
import CurrentUserContext from '../../auth/CurrentUserContext';

export default class Notifications extends React.Component<any, NotificationsState> {

    constructor(props: any){
        super(props);

        this.state ={
            isShow: false,
            messages: []
        }

        this.onClick = this.onClick.bind(this)
        this.showNotifications = this.showNotifications.bind(this)
        this.hideNotifications = this.hideNotifications.bind(this)
    }

    
    onClick(){
        this.toggleNotifications();
    }

    toggleNotifications(){
        if(this.state.isShow){
            this.hideNotifications()
        }
        else{
            this.showNotifications();
        }
    }

    showNotifications(){
        this.setState({ isShow: true })
    }
      
    hideNotifications(){        
        this.setState({ isShow: false ,messages: [] })
    }

    componentDidMount(){
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(APP_CONFIG.pushUrl, { accessTokenFactory: () => CurrentUserContext.user.jwt})
            .withAutomaticReconnect()            
            .build();
        
        connection.on("ReceivePushMessageAsync", (userId: string, message: string) => {
            
            this.state.messages.push(message);
            this.setState({
                messages: this.state.messages
            })        

            console.log(`Got push for user with id ${userId}, message: ${message}`);
        });

        this.setState({
            hunConnection: connection
        }, () => {
            connection
            .start()       
            .then(() => 
                console.log('Push connection to "ReceivePushMessageAsync" started!'))
            .catch(err => 
                console.log('Error while establishing push connection to "ReceivePushMessageAsync"'));
        })
    }

    componentWillUnmount(){
        if(this.state.hunConnection){
            this.state.hunConnection.stop();
        }        
    }
    
    public render() {
        const {isShow, messages} = this.state;
        const {hideNotifications, showNotifications, onClick} = this;

        return (            
                <Popup
                    trigger={
                        <div>
                            <Icon 
                                name="bell" 
                                title="Notifications"
                                onClick={onClick}  
                                size="big"  
                                style={{marginRight: 0}}                                         
                            />
                            {messages.length > 0 &&
                                <Label color='blue'>
                                    {messages.length}
                                </Label>
                            }
                        </div>            
                    }
                    content={
                        messages && messages.length > 0 
                        ? 
                            <Message
                                list = {messages}
                            />                        
                        :   <div>No notifications!</div>}
                    on='click'
                    open={isShow}
                    onClose={hideNotifications}
                    onOpen={showNotifications}
                    position='top right'
                />             
        )
    }
}

interface NotificationsState{
    isShow: boolean;
    messages: string[];
    hunConnection?: signalR.HubConnection;
}