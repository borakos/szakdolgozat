/*
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import 'rxjs/add/operator/do';
import { Router } from '@angular/router';

@Injectable()
export class JwtInterceptor implements HttpInterceptor{
	
	constructor(private router:Router){}

	intercept(request: HttpRequest<any>, next: HttpHandler):Observable<HttpEvent<any>>{
		return next.handle(request).do((event: HttpEvent<any>)=>{
			if(event instanceof HttpResponse){}
		},(err:any)=>{
			if(err instanceof HttpResponse){
				if(err.status === 401){
					this.router.navigate(['login']);
				}
			}
		});
	}
}
*/