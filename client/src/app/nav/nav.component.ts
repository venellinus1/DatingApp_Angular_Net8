import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, NgIf],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  private accountService = inject(AccountService);
  loggedIn = false;
  model: any = {};

  login() {
    this.accountService.login(this.model).subscribe({
      next: resposnse => {
        console.log(resposnse);
        this.loggedIn = true;
      },
      error: error => console.log(error)
    });
  }

  logout(){
    this.loggedIn = false;
  }
}
