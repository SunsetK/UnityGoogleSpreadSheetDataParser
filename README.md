# UnityGoogleSpreadSheetDataParser
Unity Google Spread Sheet Data Parser

## 개요
구글 스프레드 시트에 정의된 데이터 테이블을 .csv파일 형태로 다운받고, 게임에서 사용할 수 있는 데이터 코드를 생성해주는 에디터.
인게임에서 데이터를 인스턴스화 하는 시스템은 별개로 구현해야함.
[필수] Unity : Editor Coroutines 패키지를 인스톨 해야 사용가능.

## 사용법
### 기본 설정
1. Tools/DataParset를 클릭해 데이터 파서 에디터 창을 열기
2. Data Path 입력 : .데이터 파일이 생성되어 저장될 경로를 입력
3. File(.csv) Path 입력 : 서버로 부터 다운받은 .csv파일들이 저장될 경로를 입력.
4. Edit Templete Data Code(선택사항) : 자동으로 데이터 코드를 생성하기위한 템플릿 코드를 수정하기위해 파일을 오픈. 네임스페이스나 접두사/접미사/접근지정자등을 설정한다.

### Data 추가
1. Add Item 버튼 클릭 : 새로운 데이터를 하나 추가한다. 클릭하면 새로운 데이터의 이름과 주소를 입력할 수 있는 아이템 하나가 생성된다.
2. DataName 입력 : 생성할 데이터 이름. 구글 스프레드 시트의 시트이름과 동일해야한다. 해당이름으로 데이터 클래스가 정의되며 .cs파일이 생성된다. Perser Config의 Data Path에 이미 같은이름의 .cs파일이 존재한다면 덮어쓰며, 없다면 새로 생성한다. 
3. UriKey 입력 : 구글 스프레드시트의 uri 키(스프레드 시트 주소의 d/와 /edit~ 사이의 문자열) ex) https://docs.google.com/spreadsheets/d/1ct8um_naXjU9PM_QvLVemv2eun3MmyALhzy7wW13yy4/edit#gid=0 (주의 : 스프레드시트의 공유상태가 “링크가 있는 모든 사용자에게 공개”이어야 한다.)
4. 데이터저장을 위해 Save 버튼 클릭 :  현재 Config, Datas 설정을 저장한다.
5. Parse All 버튼 클릭 : 정의한 Data들을 모두 파싱한다.

### 기타 기능
- Help : 설명서를 연다
- Open : 해당 경로의 스프레드시트를 연다.
- Parse : 데이터 하나에 대해 파싱을 한다.
- Remove : 데이터 목록에서 해당 데이터를 제거한다.(.csv, .cs파일은 제거하지않음)

## 필요한 개선점들
- 스프레드시트 데이터 다운/읽기/쓰기 예외처리 및 로그 추가
- 코드 정리 필요
- 에디터에서 데이터 아이템이 많아지면 리스트가 잘려서 보임

## 변경점
2020.10.22 : .tsv형식에서 .csv형식을 사용하도록 수정. (유니티에서 Resources.Load를 사용할 때 .tsv 형식을 읽어오는것을 지원해주지 않아서 android에서 파일 로드가 불가능했었음.)
