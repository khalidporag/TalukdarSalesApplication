import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { TimeSetting } from 'src/app/models/time-setting.model';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-time-setting',
  templateUrl: './time-setting.component.html',
  styleUrls: ['./time-setting.component.scss']
})
export class TimeSettingComponent implements OnInit {
  time = new Date();
  public timeForm!: FormGroup;

  public selectedTime: any;
  public selectedFromTime: any;
  public selectedToTime: any;


  constructor(private fb: FormBuilder,
    private http: HttpClient,
    private toast: NgToastService,
    private api: ApiService
  ) { }

  ngOnInit() {
    // this.timeForm = this.fb.group({
    //   fromTime: [this.selectedTime.from || '', Validators.required],
    //   toTime: [this.selectedTime.to || '', Validators.required]
    // });
    this.getTimeSetting();
    this.createInit();
  }

  private createInit(): void {
    this.timeForm = this.fb.group({
      from: [this.selectedFromTime || '', Validators.required],
      to: [this.selectedToTime || '', Validators.required]
    });
  }

  getTimeSetting() {
    this.api.getTimeSetting()
      .subscribe(res => {
        this.selectedTime = res;
        this.selectedFromTime = res.from;
        this.selectedToTime = res.to;
        console.log(this.selectedTime)
      });
  }

  onSubmit() {
    const payload = {
      from: this.selectedFromTime,
      to: this.selectedToTime
    };
    this.api.updateTimeSetting(payload).subscribe(response => {
      this.toast.success({ detail: "SUCCESS", summary: response.message, duration: 5000 });
      console.log('Time saved successfully', response);
    });

  }
}