import { Component, inject, OnInit, ViewChild, viewChild } from '@angular/core';
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

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit{
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);
  images: GalleryItem[] = [];
  member: Member = {} as Member;
  activateTab?: TabDirective;
  messages: Message[] = [];
  private messageService = inject(MessageService);

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
    if(this.activateTab.heading === 'Messages' && this.messages.length === 0 && this.member){
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  // loadMember(){
  //   const username = this.route.snapshot.paramMap.get('username');

  //   if (!username) return;
  //   this.memberService.getMember(username).subscribe({
  //     next: member => {
  //       this.member = member;        
  //       member.photos.map(p => {
  //         this.images.push(new ImageItem({ src: p.url, thumb: p.url}))
  //       })
  //     }
  //   })
  // }
}
