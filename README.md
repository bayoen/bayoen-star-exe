# bayoen-star
The Puyo Puyo Tetris Add-On which counting stars and games
<p align="center">
   <img src="resources/bayoen-star-example.png"/>
</p>


# Table of Contents
- [한국어 안내](#korean)
   1. [변경점](#korean-changes)
   2. [주요기능](#korean-functions)
- [English Instruction](#english)  
   1. [Chnages](#english-changes)
   2. [Main Function](#english-functions)

* * *
<a name="korean"> </a>
# 한국어 안내
## bayoen-star-exe
'**bayoen-star**'은 뿌요뿌요 테트리스 bayoen 프로젝트의 보조기능인 별세는 앱입니다. 뿌요뿌요 테트리스 내부의 정보를 조작하지 않고, 전적분석을 위해 읽기만 합니다. 개발 및 테스트 환경은 Windows 10 (.Net Framework 4.5) 입니다.

문의 및 건의: [[디스코드](https://discord.gg/rxW5UKx)] '**SemiR4in#0001**'

시연영상: [ [YouTube 링크](https://www.youtube.com/playlist?list=PLK_vOCD9v3gUABMGU1R_VNhv5_s-LctnE) ]

다운로드: [ [bayoen-star-xxxx.zip 다운](https://github.com/bayoen/bayoen-star-exe/releases/latest) ]

<a name="korean-changes"> </a>
## 변경점
몇몇 단축키 추가
- 크로마 키 추가: 마젠타, 파랑, 초록
- 메뉴 단축키: 메인창에서 'm'을 눌러서 메뉴를 엽니다
- 초기화 단축키: 메뉴에서 'r'을 눌러서 대화상자 확인없이 초기화합니다
- 오버레이 단축키: 메뉴에서 'o'를 눌러 오버레이를 활성화 합니다

<a name="korean-functions"> </a>
## 주요기능
### 1. 카운터 및 메뉴
- 뿌요뿌요 테트리스 게임 중 '**bayoen-star**'창에 게임(왕관)/별 점수를 세어줍니다. 게임에서 별이 올라가면 카운터에 바로 반영됩니다. 게임이나 '**bayoen-star**'이 종료되어도 기록이 보존됩니다.

- 사용중 인터페이스가 깨지면, **bayoen-star** 프로그램을 **완전히 종료**하고 '**pref.json**' 이라는 파일을 삭제하고 다시 실행시키면 됩니다.

- 초기화는 '**메인창 > Menu > Reset**'입니다. 모든 수치를 초기화 합니다. 만약 게임 중이라면, 현재 별은 초기화 되지 않습니다. <단축키 M→R>

- 세부설정은 '**메인창 > Menu > Settings**'입니다. 여러가지 기능을 추가하고 있습니다. 각 항목에 마우스를 올리면 한국어 설명이 있습니다.

- 모드변경은 '**메인창 > Menu > Mode**'입니다. 모드는 **Star**(현재 별), **Star+**(누적 별), **Game**(게임/왕관)의 조합으로 구성되어 있어 편한 걸로 선택하시면 됩니다.

- 가독성을 위해서 메뉴버튼들을 하나로 합쳤습니다.
- 설정에서 통계점수를 수동조작할 수 있는 **모니터링 패널**을 추가했습니다.
- 패널 초기화 버그를 수정했습니다.  (뿌테가 켜져있을 동안 bayoen을 실행하면 버그가 발생했었습니다)

*빼먹었던 것들..*

- 동작주기를 늘렸습니다. (**50 ms**에서 **333 ms**로 바뀌었습니다; 설정파일을 수정하여 바꿀 수 있습니다만 안정성 때문에 숨겨뒀습니다)
- 트로피를 왕관으로 바꿨습니다. (뿌테에서 추출했습니다)

### 2. 오버레이
- '**bayoen-star**'창은 테두리 때문에 게임과 함께 보기 힘듭니다. 오버레이를 이용하면 게임 위에 점수판을 띄울 수 있습니다. (단, 전체화면일 경우에는 동작하지 않습니다)

- '**메인창 > Menu > Overlay**'로 오버레이를 띄울 수 있고, 오버레이를 **마우스 드래그**로 이동하고 **마우스 휠**로 크기를 조절할 수 있습니다. <단축키 M→O>

- 오버레이를 **마우스 우클릭**하면 오버레이메뉴를 볼 수 있습니다. 오버레이 메뉴의 '**Fixed**' 기능은 위치와 크기를 고정하고 게임 창이 움직일 때 함께 따라갑니다.

### 3. 송출용 도구
- 크로마키: 점수판을 방송송출하고 싶을 경우 유용한 기능입니다. 오버레이는 캡처할 수 없기 때문에 방송송출용으로는 메인창을 이용해야 합니다. 크로마키는 메인창의 색을 바꿔줍니다. '**메인창 > Menu > Settings > MAIN > Enable Chroma Key (Magenta)**'를 체크하면 메인 창의 크로마키를 활성화 할 수 있습니다. 

- 텍스트출력: 점수판 대신 직접 인터페이스를 구성하고 싶을 때 유용한 기능입니다. 송출프로그램에서 텍스트파일 (\*.txt) 파일을 불러올 수 있습니다. '**메인창 > Menu > Settings > MAIN > Export Texts**'을 체크하면 오른쪽의 '**Folder**' 경로에 파일이 저장됩니다.


* * *
<a name="english"> </a>
# English Instruction 
## bayoen-star-exe
'**bayoen-star**' is counting star module of Puyo Puyo Tetris bayeon project. We don't manipulate the information inside the Puyo Puyo Tetris, we read only it for game result analysis. The development and testing environment is Windows 10 (.Net Framework 4.5).

Contect us: [[Discord](https://discord.gg/rxW5UKx)] '**SemiR4in#0001**'

Demo video: [ [YouTube Link](https://www.youtube.com/playlist?list=PLK_vOCD9v3gUABMGU1R_VNhv5_s-LctnE) ]

Download: [ [bayoen-star-xxxx.zip File](https://github.com/bayoen/bayoen-star-exe/releases/latest) ]

<a name="english-changes"> </a>
## Changes 10(:
Add some hotkeys 
- Add few chroma keys: Magenta(Best), Blue, Green
- Menu hotkey: in main window, press 'm' to open menu
- Reset hotkey: in menu, press 'r' to reset without dialog
- Overlay hotkey: in menu, press 'o' to active overlay

<a name="english-functions"> </a>
## Functions
### 1. Counter and Menu
- '**bayoen-star**' counts crown(game)/star in Puyo Puyo Tetri game. As the stars rise in the game, they are reflected directly on the counter. records are preserved when you exit the game or '**bayoen-star**'.

- If the interface is broken during use, **Completely close bayoen-star** and '**pref.json**'. Then re-run.

- The reset is '**Main > Menu > Reset**'. Do reset all figures. If you're playing a game, the **star** doesn't reset at the moment. <Hotkey M→R>

- The detailed setting is '**Main > Menu > Settings**'. We're adding a number of features. Hover over each topic to find a Korean explanation.

- Changing display mode is '**Main > Menu > Mode**'. The mode is consister of **Star**(current star), **Star+**(cumulative star), **Game**(cumulative game/crown) and You can choose your favorites.

- Menu buttons are merged into single button for better readability
- In settings, added **monitor panel** which can change scores arbitrarily
- Fixed bugs Initializing panel (when turn on bayoen while PPT is running)

*including missing functional..*

- Increase operating time period (from **50 ms** to **333 ms**, can be tuned by modifying preferences file, we hided it for stability reason)
- Trophy is replaced by Crown (we found in PPT client)

### 2. Overlay
- The main window '**bayoen-star**' is hard to see with the game because of the border. Overlay allows you to place scoreboards on above of the game. (However, it does not work if the game is fullscreen view.)

- You can display the overlay with **main > Menu > Overlay**, Move the overlay to **Mouse Drag** and adjust the size to **Mouse Wheel**. <Hotkey M→O>

- **Mouse right click** to view the menu of overlay. The '**Fixed**' function in the Overlay menu will freeze the position and size and follow along as the Games window moves.

### 3. Tools for streaming
- Chroma Key: This is useful if you want to insert scoreboards in streaming. Because overlays cannot be captured, we should capture the main window. It changes the color of the main window for broadcast transmission. Check '**Main > Menu > Settings > MAIN > Enable Chrome Key (Magenta)**' to activate chroma key of the main window.

- Exporting Text: This is useful when you want to configure the interface directly instead of the scoreboard. You can import text files (\*.txt) from the streaming program. Check '**Main > Menu > Settings > MAIN > Export Text**' to save the file in the path of '**Folder**' on the right.
