import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/Message';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/User';
import { Group } from '../_models/group';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  hubUrl = environment.hubsUrl;
  hubConnection?: HubConnection;
  messageThread = signal<Message[]>([]);
  private busyService = inject(BusyService);

  createHubConnection(user: User, otherUsername: string){
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    this.hubConnection.start()
      .catch(error => console.log(error))
      .finally(() => this.busyService.idle()); 

    //send back messages - !!! Careful here - ReceiveMessageThread should match the  method name on API ReceiveMessageThread
    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages);
    })

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message]);
    })

    this.hubConnection.on('UpdatedGroup', (group:Group) => {
      if(group.connections.some(x => x.username === otherUsername)){
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead){
              message.dateRead = new Date(Date.now());
            }
          })
          return messages;
        })
      }
    })
  }

  stopHubConnection(){
    if (this.hubConnection?.state === HubConnectionState.Connected){
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string){
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('Container', container);
    return this.http.get<Message[]>(this.baseUrl + 'messages', {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult),
    })
      
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async sendMessage(username: string, content: string){//use async to avoid returning Promise<> | undefined - with async its just Promise<>
    // SendMessage should match the name in MessageHub.cs
    return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content}); 
  }

  deleteMessage(id: number){
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
