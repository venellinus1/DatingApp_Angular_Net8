import { Component, inject, OnDestroy, OnInit, ViewChild, viewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/Member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/Message';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, OnDestroy{
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  
  private route = inject(ActivatedRoute);
  images: GalleryItem[] = [];
  member: Member = {} as Member;
  activateTab?: TabDirective;
  private messageService = inject(MessageService);
  presenceService = inject(PresenceService);
  private accountService = inject(AccountService);

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({ src: p.url, thumb: p.url}))
        })
      }
    })
    this.route.queryParamMap.subscribe({
      next: params => {
        const tab = params.get('tab');
        if (tab) {
          this.selectTab(tab);
        }
      }
    })
  }

  selectTab(heading: string){
    if(this.memberTabs){
      const messageTab = this.memberTabs.tabs.find(x => x.heading === heading);
      if(messageTab) messageTab.active = true;      
    }
  }

  onTabActivated(data: TabDirective){
    this.activateTab = data;
    if(this.activateTab.heading === 'Messages' && this.member){
      const user = this.accountService.currentUser();
      if (!user) return;
      
      this.messageService.createHubConnection(user, this.member.userName); // user= current user,  this.member.userName= other user in the chat
    } else {
      this.messageService.stopHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
