import { Component, inject, input, OnInit } from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { Message } from '../../_models/Message';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements OnInit {
  username = input.required<string>();
  private messageService = inject(MessageService);

  messages:Message[] = [];

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages(){
    this.messageService.getMessageThread(this.username()).subscribe({
      next: messages => this.messages = messages
    })
  }
}
