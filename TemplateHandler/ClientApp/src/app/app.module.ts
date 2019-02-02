import { BrowserModule } from '@angular/platform-browser';
import { NgModule, Injector } from '@angular/core';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RouterModule } from '@angular/router';
import { UsersComponent } from './users/users.component';
import { FormsModule } from '@angular/forms';
import { JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { HttpClientModule } from '@angular/common/http';
import { AuthGuard } from './guards/auth-services';
import { AdminGuard } from './guards/admin-auth-services';
import { NavComponent } from './nav/nav.component';
import { createCustomElement } from '@angular/elements';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HomeComponent,
    UsersComponent,
    NavComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    JwtModule.forRoot({
      config:{
        throwNoTokenError: false,
        tokenGetter: getToken,
        whitelistedDomains: ["localhost:44396"]
      }
    }),
    RouterModule.forRoot([
      {
        path:'',
        component:HomeComponent,
        canActivate: [AuthGuard]
      },
      {
        path:'login',
        component: LoginComponent
      },
      {
        path:'users',
        component:UsersComponent,
        canActivate: [AuthGuard && AdminGuard]
      }
    ])
  ],
  providers: [JwtHelperService, AuthGuard, AdminGuard],
  bootstrap: [AppComponent]
})
export class AppModule { 
  constructor(private injector:Injector){}

  ngDoBootstrap(){
    const customNav=createCustomElement(NavComponent,{injector: this.injector});
    customElements.define('app-nav',customNav);
  }
}

export function getToken(){
  return localStorage.getItem("currentUser");
}
