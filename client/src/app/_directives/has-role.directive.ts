import { Directive, inject, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]',//*appHasRole
  standalone: true
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[] = [];
  private accountService = inject(AccountService);
  private viewContainerRef = inject(ViewContainerRef);//container where one or more views can be attached to a component
  private templateRef = inject(TemplateRef);

  ngOnInit(): void {
    if (this.accountService.roles().some((r: string) => this.appHasRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear();//remove that ref from the view - not for this user's role
    }
  }
}
