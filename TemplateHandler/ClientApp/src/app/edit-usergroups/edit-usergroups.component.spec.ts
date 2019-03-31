import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditUsergroupsComponent } from './edit-usergroups.component';

describe('EditUsergroupsComponent', () => {
  let component: EditUsergroupsComponent;
  let fixture: ComponentFixture<EditUsergroupsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditUsergroupsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditUsergroupsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
